using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float shiftSpeed;
    [SerializeField] private float sprintSpeed;
    [Header("Stamina")]
    [SerializeField] private float maxStamina;
    [SerializeField] private float minStamina;
    [SerializeField] private float staminaConsumption;
    [SerializeField] private float staminaRegeneration;
    [SerializeField] private Image staminaBar;
    [Header("Other")]
    [SerializeField] private Tilemap groundTilemap;

    public PlayerFacing Facing { get; private set; } = PlayerFacing.Right;
    public bool Frozen { private get; set; }
    
    private Rigidbody2D _rigidBody;
    private float _stamina;

    private void Start()
    {
        _stamina = maxStamina;
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HandleMovement();
        HandleFall();
    }

    private void HandleMovement()
    {
        if (Frozen)
        {
            _rigidBody.velocity = Vector2.zero;
            return;
        }
        
        var sprinted = false;
        
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Move(shiftSpeed);
        }
        else
        {
            if (_stamina > minStamina + 0.1f && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                _stamina -= staminaConsumption * Time.deltaTime;
                Move(sprintSpeed);
                sprinted = true;
            }
            else
            {
                Move(movementSpeed);
            }
        }

        if (!sprinted)
        {
            _stamina = Mathf.Clamp(_stamina + staminaRegeneration * Time.deltaTime, 0f, maxStamina);
        }

        staminaBar.fillAmount = Mathf.Clamp(_stamina / maxStamina, 0f, 1f);
    }

    private void HandleFall()
    {
        var gridX = Mathf.RoundToInt(transform.position.x);
        var gridY = Mathf.RoundToInt(transform.position.y);

        if (groundTilemap.GetTile(new Vector3Int(gridX, gridY, 0)) == null)
        {
            Debug.Log($"The player stepped on a non-existing tile at x={gridX}; y={gridY}");
            
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
        }
    }

    private void Move(float speed)
    {
        var horizontal = Input.GetAxis("Horizontal") * speed;
        var vertical = Input.GetAxis("Vertical") * speed;
        _rigidBody.velocity = new Vector2(horizontal, vertical);

        if (horizontal > 0f)
        {
            Facing = PlayerFacing.Right;
        }
        else
        {
            if (horizontal < 0f)
            {
                Facing = PlayerFacing.Left;
            }
        }
    }
}

public enum PlayerFacing
{
    Left,
    Right
}
