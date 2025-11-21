using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float speed = -15f;
    public Vector3 direction = new Vector3(0, 1);

	void Update ()
    {
        this.transform.eulerAngles += direction * Time.deltaTime * speed;
	}
}
