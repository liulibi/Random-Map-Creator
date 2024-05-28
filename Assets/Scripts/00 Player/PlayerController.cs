using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : LivingEntity
{
    private Rigidbody rb;
    private Vector3 moveInput;
    [SerializeField] private float moveSpeed;

    public Crosshairs crosshairs;//MARKER 鼠标光标位置

    protected override void Start() 
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //moveInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        //LookAtCursor();

        //if (transform.position.y < -10)//如果玩家掉下去的话，就GameOver了
        //    TakenDamage(health);
    }

    private void FixedUpdate()
    {
        rb.velocity = moveInput.normalized * moveSpeed;
    }

    private void LookAtCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Plane plane = new Plane(Vector3.up, Vector3.zero);
        Plane plane = new Plane(Vector3.up, Vector3.up * FindObjectOfType<GunController>().GetHeight);

        float distToGround;
        if (plane.Raycast(ray, out distToGround))
        {
            Vector3 point = ray.GetPoint(distToGround);
            Vector3 rightPoint = new Vector3(point.x, transform.position.y, point.z);

            transform.LookAt(rightPoint);
            crosshairs.transform.position = point;//MARKER 鼠标位置

            crosshairs.DetectTargets(ray);//MARKER 调用DetectTargets方法，鼠标在敌人上方则变色
        }
    }
}
