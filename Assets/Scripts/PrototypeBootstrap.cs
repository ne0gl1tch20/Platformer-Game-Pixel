using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Playable platformer prototype using the imported Kenney Adventurer prefab and Grass tiles.
/// Gameplay objects are created here, but all character and terrain artwork comes from source assets.
/// </summary>
public sealed class PrototypeBootstrap : MonoBehaviour
{
    private const float MoveSpeed = 5.2f;
    private const float JumpForce = 8.6f;
    private const int TotalStars = 6;

    [Header("Kenney assets")]
    [SerializeField] private GameObject adventurerPrefab;
    [SerializeField] private TileBase grassLeftTile;
    [SerializeField] private TileBase grassMiddleTile;
    [SerializeField] private TileBase grassRightTile;

    private readonly List<Transform> pickups = new List<Transform>();
    private Rigidbody2D body;
    private Transform hero;
    private Animator animator;
    private Camera gameCamera;
    private float moveInput;
    private float touchMoveInput;
    private bool jumpQueued;
    private int stars;
    private bool finished;
    private Vector2 spawnPoint = new Vector2(-6f, -0.35f);
    private GUIStyle hudStyle;
    private GUIStyle buttonStyle;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Debug.Log("[PrototypeBootstrap] Startup began.");

        if (adventurerPrefab == null)
        {
            Debug.LogError("[PrototypeBootstrap] Adventurer Prefab is not assigned. The player cannot spawn.");
            enabled = false;
            return;
        }

        Debug.Log("[PrototypeBootstrap] Adventurer prefab reference is assigned.");

        // Spawn the player before optional world setup so a terrain/pickup error
        // cannot prevent the character from appearing in the Hierarchy.
        RunStartupStep("Adventurer spawn", BuildHero);
        RunStartupStep("Camera setup", BuildCamera);

        if (grassLeftTile == null || grassMiddleTile == null || grassRightTile == null)
        {
            Debug.LogError("[PrototypeBootstrap] One or more grass Tile references are missing. Skipping environment creation; the Adventurer spawn was attempted independently.");
        }
        else
        {
            RunStartupStep("Environment creation", BuildWorld);
        }

        RunStartupStep("Pickup creation", BuildPickups);
        Debug.Log("[PrototypeBootstrap] Startup sequence finished. Check earlier errors for any skipped step.");
    }

    private static void RunStartupStep(string stepName, System.Action action)
    {
        Debug.Log("[PrototypeBootstrap] Starting: " + stepName + ".");
        try
        {
            action();
            Debug.Log("[PrototypeBootstrap] Completed: " + stepName + ".");
        }
        catch (System.Exception exception)
        {
            Debug.LogError("[PrototypeBootstrap] Failed during " + stepName + ": " + exception);
        }
    }

    private void BuildWorld()
    {
        GameObject gridObject = new GameObject("Kenney Environment Grid");
        Grid grid = gridObject.AddComponent<Grid>();
        grid.cellSize = Vector3.one;

        GameObject tilemapObject = new GameObject("Kenney Grass Tilemap");
        tilemapObject.transform.SetParent(gridObject.transform, false);
        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        TilemapRenderer tilemapRenderer = tilemapObject.AddComponent<TilemapRenderer>();
        tilemapRenderer.sortingOrder = 0;
        TilemapCollider2D tilemapCollider = tilemapObject.AddComponent<TilemapCollider2D>();
        tilemapCollider.usedByComposite = false;

        // Platform layout is gameplay data. Every visible ground tile is a Kenney Tile asset.
        CreatePlatform(tilemap, -10, -3, -2);
        CreatePlatform(tilemap, -1, 2, -1);
        CreatePlatform(tilemap, 4, 7, 1);
        CreatePlatform(tilemap, 9, 12, 0);
        CreatePlatform(tilemap, 14, 18, 2);
        CreatePlatform(tilemap, 20, 25, 0);

        GameObject goal = new GameObject("Goal Marker");
        goal.transform.position = new Vector3(16f, 3.65f, 0f);
        TextMesh goalText = goal.AddComponent<TextMesh>();
        goalText.text = "GOAL";
        goalText.anchor = TextAnchor.MiddleCenter;
        goalText.alignment = TextAlignment.Center;
        goalText.fontSize = 48;
        goalText.characterSize = 0.12f;
        goalText.color = new Color(1f, 0.88f, 0.35f);
        goalText.GetComponent<MeshRenderer>().sortingOrder = 10;
    }

    private void CreatePlatform(Tilemap tilemap, int startX, int endX, int rowY)
    {
        for (int x = startX; x <= endX; x++)
        {
            TileBase tile = x == startX
                ? grassLeftTile
                : x == endX
                    ? grassRightTile
                    : grassMiddleTile;

            tilemap.SetTile(new Vector3Int(x, rowY, 0), tile);
        }
    }

    private void BuildPickups()
    {
        pickups.Clear();
        Vector2[] positions =
        {
            new Vector2(-1f, 0.75f),
            new Vector2(2f, 1.15f),
            new Vector2(5.5f, 2.65f),
            new Vector2(10f, 1.65f),
            new Vector2(15f, 3.65f),
            new Vector2(22f, 1.65f)
        };

        foreach (Vector2 position in positions)
        {
            GameObject pickup = new GameObject("Star Pickup");
            pickup.transform.position = position;
            TextMesh star = pickup.AddComponent<TextMesh>();
            star.text = "★";
            star.anchor = TextAnchor.MiddleCenter;
            star.alignment = TextAlignment.Center;
            star.fontSize = 64;
            star.characterSize = 0.16f;
            star.color = new Color(1f, 0.82f, 0.18f);
            star.GetComponent<MeshRenderer>().sortingOrder = 10;
            pickups.Add(pickup.transform);
        }
    }

    private void BuildHero()
    {
        GameObject player = Instantiate(adventurerPrefab, spawnPoint, Quaternion.identity);
        player.name = "Kenney Adventurer";
        player.transform.localScale = Vector3.one * 0.18f;

        hero = player.transform;
        body = player.GetComponent<Rigidbody2D>();
        animator = player.GetComponent<Animator>();

        if (body == null)
        {
            Debug.LogError("The Adventurer prefab is missing its Rigidbody2D.");
            enabled = false;
            return;
        }

        body.freezeRotation = true;
        body.gravityScale = 3f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void BuildCamera()
    {
        gameCamera = Camera.main;
        if (gameCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            gameCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        gameCamera.orthographic = true;
        gameCamera.orthographicSize = 5.2f;
        gameCamera.backgroundColor = new Color32(128, 204, 255, 255);
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.transform.position = new Vector3(0f, 1f, -10f);
    }

    private void Update()
    {
        if (hero == null || body == null) return;

        if (Input.GetKeyDown(KeyCode.R)) Restart();

        float keyboardInput = Input.GetAxisRaw("Horizontal");
        moveInput = Mathf.Abs(touchMoveInput) > 0.1f ? touchMoveInput : keyboardInput;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpQueued = true;
        }

        bool grounded = Physics2D.Raycast(body.position, Vector2.down, 0.62f).collider != null;
        if (jumpQueued && grounded && !finished)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, JumpForce);
        }
        jumpQueued = false;

        body.linearVelocity = new Vector2(finished ? 0f : moveInput * MoveSpeed,
            body.linearVelocity.y);

        SpriteRenderer spriteRenderer = hero.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && Mathf.Abs(moveInput) > 0.05f)
        {
            spriteRenderer.flipX = moveInput < 0f;
        }

        if (animator != null)
        {
            animator.SetFloat("MoveSpeed", Mathf.Abs(moveInput));
            animator.SetBool("Grounded", grounded);
            animator.SetFloat("VerticalVelocity", body.linearVelocity.y);
        }

        for (int i = pickups.Count - 1; i >= 0; i--)
        {
            Transform pickup = pickups[i];
            if (pickup == null)
            {
                pickups.RemoveAt(i);
                continue;
            }

            if (Vector2.Distance(hero.position, pickup.position) < 0.8f)
            {
                stars++;
                Destroy(pickup.gameObject);
                pickups.RemoveAt(i);
            }
        }

        if (!finished && hero.position.x > 15.4f && hero.position.y > 2f)
        {
            finished = true;
            body.linearVelocity = Vector2.zero;
        }

        if (hero.position.y < -8f) Restart();

        if (gameCamera != null)
        {
            Vector3 target = new Vector3(hero.position.x + 2f,
                Mathf.Clamp(hero.position.y + 1f, 0.2f, 3.2f), -10f);
            gameCamera.transform.position = Vector3.Lerp(gameCamera.transform.position,
                target, Time.deltaTime * 4f);
        }
    }

    private void Restart()
    {
        stars = 0;
        finished = false;

        foreach (Transform pickup in pickups)
        {
            if (pickup != null) Destroy(pickup.gameObject);
        }

        BuildPickups();

        if (hero != null) hero.position = spawnPoint;
        if (body != null) body.linearVelocity = Vector2.zero;
    }

    private void OnGUI()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(18, Screen.height / 28),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.12f, 0.2f, 0.3f) }
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Max(20, Screen.height / 24),
                fontStyle = FontStyle.Bold
            };
        }

        GUI.Label(new Rect(18, 12, 380, 48), "PIXEL HOP   ★ " + stars + "/" + TotalStars, hudStyle);
        if (finished)
        {
            GUI.Label(new Rect(Screen.width * 0.35f, 50, Screen.width * 0.4f, 60),
                "LEVEL CLEAR! ✨", hudStyle);
        }

        float size = Mathf.Clamp(Screen.height * 0.14f, 64f, 110f);
        float y = Screen.height - size - 22f;
        touchMoveInput = 0f;

        if (GUI.RepeatButton(new Rect(22f, y, size, size), "◀", buttonStyle))
            touchMoveInput = -1f;
        if (GUI.RepeatButton(new Rect(30f + size, y, size, size), "▶", buttonStyle))
            touchMoveInput = 1f;
        if (GUI.Button(new Rect(Screen.width - size - 24f, y, size, size), "JUMP", buttonStyle))
            jumpQueued = true;
        if (GUI.Button(new Rect(Screen.width - 70f, 16f, 52f, 44f), "↻", buttonStyle))
            Restart();
    }
}
