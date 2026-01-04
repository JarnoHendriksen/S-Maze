using System;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class PlayerController : MonoBehaviour
{
    public MazeBuilder mazeBuilder;

    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    Vector2 moveDirection;
    Vector2 mousePosition;

    bool isPlayerCloseToDoor = false;
    public float proximityThreshold = 1.0f;

    public bool IsInWall { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IsInWall = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameManager.instance.GamePaused = !GameManager.instance.GamePaused;
            UIHandler.instance.PauseBtnClick();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIHandler.instance.SettingsBtnClick();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UIHandler.instance.PuzzleCompleted();
            UIHandler.instance.ShowTextPrompt("Puzzle Completed!");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.instance.LoadNextLevel();
        }

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

        // Able to open doors when close enough
        GameObject[] Doors = GameObject.FindGameObjectsWithTag("door");
        GameObject theDoor = mazeBuilder.closestDoor(Doors, rb.position, proximityThreshold);

        if (theDoor != null)
        {
            // Now it is safe to check the distance
            float distance = Vector2.Distance(theDoor.transform.position, rb.position);

            isPlayerCloseToDoor = true;

            if (Input.GetKeyDown(KeyCode.E))
            {
                mazeBuilder.openDoor(theDoor);
            }
            
        }
        else
        {
            // No door was found within range
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
            // If all puzzles completed: pause game & trigger level completed screen
            // Else: Show prompt to complete all levels
            UIHandler.instance.ShowTextPrompt("You need to complete all puzzles before you can leave!");
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
