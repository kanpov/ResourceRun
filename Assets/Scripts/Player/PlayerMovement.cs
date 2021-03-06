using System.Collections;
using ResourceRun.World.Generation;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace ResourceRun.Player
{
    /// <summary>
    /// The script responsible for all types of player movement and for the player's stamina (consumption, regeneration and HUD display),
    /// as it's directly related to the Ctrl + WASD movement type.
    ///
    /// This script also handles checking whether the player is on an existing ground tile. If not, it proceeds to generate the next season
    /// or finish the game session if the season was winter.
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] [Tooltip("The speed of the regular type of movement (WASD)")]
        private float movementSpeed;
        [SerializeField] [Tooltip("The speed of the slowed down movement type (Shift + WASD)")]
        private float shiftSpeed;
        [SerializeField] [Tooltip("The speed of the sped up movement type that uses up stamina (Ctrl + WASD)")]
        private float sprintSpeed;
        
        [Header("Stamina")] 
        [SerializeField] [Tooltip("The highest amount of stamina the player can have")]
        private float maxStamina;
        [SerializeField] [Tooltip("The lowest amount of stamina the player can have")]
        private float minStamina;
        [SerializeField] [Tooltip("The amount of stamina that is consumed when using Ctrl + WASD movement. Multiplied by Time.deltaTime")]
        private float staminaConsumption;
        [SerializeField] [Tooltip("The amount of stamina that is regenerated when not using Ctrl + WASD movement. Multiplied by Time.deltaTime")]
        private float staminaRegeneration;
        [SerializeField] [Tooltip("The stamina bar filled Image at the front (not the background Image)")]
        private Image staminaBar;
        
        [Header("Other")]
        [SerializeField] [Tooltip("The Tilemap with the ground tiles to check whether the player is standing on a ground tile")]
        private Tilemap groundTilemap;
        [SerializeField] [Range(0.1f, 1f)] [Tooltip("How fast will the falling animation be")]
        private float fallSpeed = 0.25f;

        private Rigidbody2D _rigidBody;
        private WorldGenerator _generator;
        private SpriteRenderer _spriteRenderer;
        private float _stamina;
        private bool _immuneFromFall;

        /// <summary>
        /// The current horizontal facing of the player, can be left or right
        /// </summary>
        public PlayerFacing Facing { get; private set; } = PlayerFacing.Right;
        
        /// <summary>
        /// A flag that can be toggled on and off to completely block any movement from the player and any stamina regeneration
        /// </summary>
        public bool Frozen { private get; set; }

        private void Start()
        {
            _stamina = maxStamina;
            _rigidBody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _generator = FindObjectOfType<WorldGenerator>();
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
                if (_stamina > minStamina + 0.1f &&
                    (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
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

            if (!sprinted) _stamina = Mathf.Clamp(_stamina + staminaRegeneration * Time.deltaTime, 0f, maxStamina);

            staminaBar.fillAmount = Mathf.Clamp(_stamina / maxStamina, 0f, 1f);
        }

        private void HandleFall()
        {
            if (_immuneFromFall) return;
            
            var gridX = Mathf.FloorToInt(transform.position.x);
            var gridY = Mathf.FloorToInt(transform.position.y);

            if (groundTilemap.GetTile(new Vector3Int(gridX, gridY, 0)) == null)
            {
                Debug.Log($"The player stepped on a non-existing tile at x={gridX}; y={gridY}");
                StartCoroutine(Fall(gridX, gridY));
            }
        }

        private IEnumerator Fall(float gridX, float gridY)
        {
            Frozen = true;
            _immuneFromFall = true;
            
            transform.position = new Vector3(gridX + 0.5f, gridY + 0.5f, transform.position.z);
            
            while (transform.localScale.x > 0.25f)
            {
                transform.localScale = new Vector3(
                    transform.localScale.x - 0.01f * fallSpeed,
                    transform.localScale.y - 0.01f * fallSpeed, transform.localScale.z);
                yield return new WaitForSeconds(0.01f);
            }
            
            _spriteRenderer.enabled = false;

            _generator.GenerateNextSeason();

            _spriteRenderer.enabled = true;
            transform.localScale = Vector3.one;
            Frozen = false;
            _immuneFromFall = false;
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
                if (horizontal < 0f) Facing = PlayerFacing.Left;
            }
        }
    }

    public enum PlayerFacing
    {
        Left,
        Right
    }
}