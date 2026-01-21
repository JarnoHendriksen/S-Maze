using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public MazeBuilder mazeBuilder;

    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    Vector2 moveDirection;
    Vector2 mousePosition;
    private int currentLevel = 1;

    bool isPlayerCloseToDoor = false;
    public float proximityThreshold = 1.0f;

    public bool IsInWall { get; private set; }

    void Start()
    {
        IsInWall = false;
    }

    void Update()
    {
        // DELETED the "Escape" key check. 
        // DELETED space interaction
        // The UIHandler now handles "Escape" automatically in its own
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentLevel++;
            StartCoroutine(GameManager.instance.LoadNextLevel());
        }

        rb.linearVelocity = Vector2.zero;

        if (GameManager.instance.GamePaused) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
        rb.angularVelocity = 0f;

        Vector2 aimDirection = mousePosition - rb.position;
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = aimAngle;

        GameObject[] Doors = GameObject.FindGameObjectsWithTag("door");
        GameObject theDoor = mazeBuilder.closestDoor(Doors, rb.position, proximityThreshold);

        if (theDoor != null)
        {
            float distance = Vector2.Distance(theDoor.transform.position, rb.position);
            isPlayerCloseToDoor = true;

            if (Input.GetKeyDown(KeyCode.E) && isPlayerCloseToDoor)
            {
                mazeBuilder.openDoor(theDoor);
            }
        }
        else
        {
            isPlayerCloseToDoor = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("fake_wall_parent") && !IsInWall)
        {
            IsInWall = true;
        }
        else if (collision.CompareTag("item"))
        {
            ItemData idat = collision.gameObject.GetComponent<ItemData>();
            PlayerInventory.instance.CollectItem(idat);
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("exit"))
        {
            if (GameManager.instance.LevelCompleted)
            {
                if (AudioSystem.instance != null) AudioSystem.instance.PlaySoundEffect(SoundEffectType.MazeCompleted);
                UIHandler.instance.ShowLevelCompletedScreen();
                DataCollector.Instance.LevelUp(GameManager.instance.Level);
            }
            else
            {
                if (AudioSystem.instance != null) AudioSystem.instance.PlaySoundEffect(SoundEffectType.InvalidAction);
                UIHandler.instance.ShowTextPrompt("You need to complete all puzzles before you can leave!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("fake_wall_parent") && IsInWall)
        {
            IsInWall = false;
        }
    }
}