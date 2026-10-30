using UnityEngine;

/// <summary>
/// One-file, dependency-free first playable slice. Add this component to an empty GameObject and press Play.
/// All prototype art is generated as tiny point-filtered textures at runtime.
/// </summary>
public class PrototypeBootstrap : MonoBehaviour
{
    const float MoveSpeed = 5.2f;
    const float JumpForce = 10.5f;
    Rigidbody2D body;
    Transform hero;
    Camera gameCamera;
    Texture2D heroTexture, groundTexture, starTexture, cloudTexture;
    float moveInput;
    float touchMoveInput;
    bool jumpQueued;
    int stars;
    bool finished;
    Vector2 spawnPoint = new Vector2(-6f, -0.3f);
    GUIStyle hudStyle, buttonStyle;

    void Start()
    {
        Application.targetFrameRate = 60;
        BuildArt();
        BuildWorld();
        BuildHero();
        BuildCamera();
    }

    void BuildArt()
    {
        heroTexture = NewTexture(16, 20, new Color32(0,0,0,0));
        Rect(heroTexture, 3, 13, 10, 5, new Color32(235,75,105,255)); // cap
        Rect(heroTexture, 5, 10, 8, 5, new Color32(255,205,145,255)); // face
        Rect(heroTexture, 4, 8, 9, 3, new Color32(70,145,235,255)); // shirt
        Rect(heroTexture, 3, 2, 4, 6, new Color32(55,65,105,255)); // left leg
        Rect(heroTexture, 9, 2, 4, 6, new Color32(55,65,105,255)); // right leg
        Rect(heroTexture, 10, 12, 1, 1, Color.black); // eye
        Rect(heroTexture, 1, 14, 7, 2, new Color32(235,75,105,255));
        heroTexture.Apply();

        groundTexture = NewTexture(16, 16, new Color32(126,78,55,255));
        Rect(groundTexture, 0, 12, 16, 4, new Color32(93,195,91,255));
        Rect(groundTexture, 0, 12, 16, 1, new Color32(166,231,105,255));
        for (int x = 2; x < 16; x += 5) Rect(groundTexture, x, 4, 2, 2, new Color32(153,100,67,255));
        groundTexture.Apply();

        starTexture = NewTexture(16, 16, new Color32(0,0,0,0));
        Rect(starTexture, 7, 1, 2, 14, new Color32(255,195,45,255));
        Rect(starTexture, 3, 4, 10, 8, new Color32(255,195,45,255));
        Rect(starTexture, 5, 6, 6, 4, new Color32(255,239,119,255));
        starTexture.Apply();

        cloudTexture = NewTexture(32, 16, new Color32(0,0,0,0));
        Rect(cloudTexture, 4, 3, 24, 8, new Color32(255,255,255,225));
        Rect(cloudTexture, 8, 7, 8, 7, new Color32(255,255,255,225));
        Rect(cloudTexture, 16, 9, 9, 5, new Color32(255,255,255,225));
        cloudTexture.Apply();
    }

    Texture2D NewTexture(int w, int h, Color fill)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        t.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = fill;
        t.SetPixels(pixels);
        return t;
    }

    void Rect(Texture2D t, int x, int y, int w, int h, Color c)
    {
        for (int px = x; px < x + w; px++)
            for (int py = y; py < y + h; py++)
                if (px >= 0 && py >= 0 && px < t.width && py < t.height) t.SetPixel(px, py, c);
    }

    Sprite MakeSprite(Texture2D texture, float pixelsPerUnit = 16f)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    GameObject MakeBlock(string label, Vector2 position, Vector2 size, Sprite sprite, bool solid)
    {
        var go = new GameObject(label);
        go.transform.position = position;
        go.transform.localScale = new Vector3(size.x, size.y, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        if (solid) {
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }
        return go;
    }

    void BuildWorld()
    {
        var bg = new GameObject("Sky");
        bg.transform.position = new Vector3(0, 0, 10);
        var cameraBg = bg.AddComponent<Camera>();
        cameraBg.orthographic = true;
        cameraBg.orthographicSize = 5.5f;
        cameraBg.clearFlags = CameraClearFlags.SolidColor;
        cameraBg.backgroundColor = new Color32(128, 204, 255, 255);
        cameraBg.depth = -2;
        cameraBg.enabled = false;

        var ground = MakeSprite(groundTexture);
        MakeBlock("Starting Island", new Vector2(-6, -2), new Vector2(8, 1), ground, true);
        MakeBlock("Low Platform", new Vector2(0, -0.5f), new Vector2(3, 0.55f), ground, true);
        MakeBlock("High Platform", new Vector2(4, 1.2f), new Vector2(2.8f, 0.55f), ground, true);
        MakeBlock("Landing Island", new Vector2(8.2f, 0), new Vector2(4, 0.8f), ground, true);
        MakeBlock("Finish Island", new Vector2(13, 1.8f), new Vector2(4, 0.8f), ground, true);
        MakeBlock("Far Island", new Vector2(18, 0.1f), new Vector2(5, 0.8f), ground, true);

        for (int i = 0; i < 6; i++)
        {
            float x = -2f + i * 3.4f;
            var star = new GameObject("Star");
            star.transform.position = new Vector3(x, i % 2 == 0 ? 1.0f : 2.8f, 0);
            star.transform.localScale = Vector3.one * 0.75f;
            star.AddComponent<SpriteRenderer>().sprite = MakeSprite(starTexture);
        }

        for (int i = 0; i < 7; i++)
        {
            var cloud = new GameObject("Cloud");
            cloud.transform.position = new Vector3(-7 + i * 4.8f, 3.4f + (i % 3) * 1.0f, 2);
            cloud.transform.localScale = Vector3.one * (1.2f + (i % 2) * 0.6f);
            var renderer = cloud.AddComponent<SpriteRenderer>();
            renderer.sprite = MakeSprite(cloudTexture);
            renderer.sortingOrder = -1;
        }

        var flag = MakeBlock("Finish Flag", new Vector2(13, 3f), new Vector2(0.22f, 2.2f),
            MakeSprite(NewTexture(4, 32, new Color32(255, 120, 120, 255))), false);
        flag.GetComponent<SpriteRenderer>().color = new Color32(255, 125, 125, 255);
    }

    void BuildHero()
    {
        var go = new GameObject("Pixel Hero");
        go.transform.position = spawnPoint;
        hero = go.transform;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MakeSprite(heroTexture);
        sr.sortingOrder = 5;
        body = go.AddComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.gravityScale = 2.8f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.72f, 0.92f);
    }

    void BuildCamera()
    {
        gameCamera = Camera.main;
        if (gameCamera == null) {
            var go = new GameObject("Main Camera");
            gameCamera = go.AddComponent<Camera>();
            go.tag = "MainCamera";
        }
        gameCamera.orthographic = true;
        gameCamera.orthographicSize = 5.2f;
        gameCamera.transform.position = new Vector3(0, 1, -10);
        gameCamera.backgroundColor = new Color32(128, 204, 255, 255);
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    void Update()
    {
        if (hero == null || body == null) return;
        if (Input.GetKeyDown(KeyCode.R)) Restart();
        float keyboardInput = Input.GetAxisRaw("Horizontal");
        moveInput = Mathf.Abs(touchMoveInput) > 0.1f ? touchMoveInput : keyboardInput;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            jumpQueued = true;

        var hits = Physics2D.OverlapCircleAll(hero.position + Vector3.down * 0.55f, 0.18f);
        bool grounded = false;
        foreach (var hit in hits) if (hit.gameObject != hero.gameObject && hit.GetComponent<BoxCollider2D>() != null) grounded = true;
        if (jumpQueued && grounded && !finished) body.linearVelocity = new Vector2(body.linearVelocity.x, JumpForce);
        jumpQueued = false;
        body.linearVelocity = new Vector2(finished ? 0 : moveInput * MoveSpeed, body.linearVelocity.y);

        var sprite = hero.GetComponent<SpriteRenderer>();
        if (Mathf.Abs(moveInput) > 0.05f) sprite.flipX = moveInput < 0;

        foreach (var item in GameObject.FindGameObjectsWithTag("Untagged"))
        {
            if (item.name == "Star" && Vector2.Distance(hero.position, item.transform.position) < 0.75f) {
                stars++;
                Destroy(item);
            }
        }

        if (!finished && hero.position.x > 12.2f && hero.position.y > 0.8f) {
            finished = true;
            body.linearVelocity = Vector2.zero;
        }
        if (hero.position.y < -8f) Restart();

        if (gameCamera != null) {
            var target = new Vector3(hero.position.x + 2.0f, Mathf.Clamp(hero.position.y + 1.0f, 0.2f, 2.5f), -10);
            gameCamera.transform.position = Vector3.Lerp(gameCamera.transform.position, target, Time.deltaTime * 4f);
        }
    }

    void Restart()
    {
        stars = 0;
        finished = false;
        foreach (var item in GameObject.FindGameObjectsWithTag("Untagged"))
            if (item.name == "Star") Destroy(item);
        // Recreate the six original pickups.
        for (int i = 0; i < 6; i++) {
            var star = new GameObject("Star");
            star.transform.position = new Vector3(-2f + i * 3.4f, i % 2 == 0 ? 1.0f : 2.8f, 0);
            star.transform.localScale = Vector3.one * 0.75f;
            star.AddComponent<SpriteRenderer>().sprite = MakeSprite(starTexture);
        }
        hero.position = spawnPoint;
        body.linearVelocity = Vector2.zero;
    }

    void OnGUI()
    {
        if (hudStyle == null) {
            hudStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(18, Screen.height / 28), fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.12f, 0.2f, 0.3f) } };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Max(20, Screen.height / 24), fontStyle = FontStyle.Bold };
        }
        GUI.Label(new Rect(18, 12, 380, 48), "PIXEL HOP   ★ " + stars + "/6", hudStyle);
        if (finished) GUI.Label(new Rect(Screen.width * 0.35f, 50, Screen.width * 0.4f, 60), "LEVEL CLEAR! ✨", hudStyle);

        float size = Mathf.Clamp(Screen.height * 0.14f, 64, 110);
        float y = Screen.height - size - 22;
        touchMoveInput = 0;
        if (GUI.RepeatButton(new Rect(22, y, size, size), "◀", buttonStyle)) touchMoveInput = -1;
        if (GUI.RepeatButton(new Rect(30 + size, y, size, size), "▶", buttonStyle)) touchMoveInput = 1;
        if (GUI.Button(new Rect(Screen.width - size - 24, y, size, size), "JUMP", buttonStyle)) jumpQueued = true;
        if (GUI.Button(new Rect(Screen.width - 70, 16, 52, 44), "↻", buttonStyle)) Restart();
    }
}