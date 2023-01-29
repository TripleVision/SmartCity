using UnityEngine;
using System.Collections;
 
public class BasicCarController : MonoBehaviour
{
	float speed = 5f; // Speed of the car
	float turnSpeed = 180f; // Turn speed

	private float m_horizontalInput;
	private float m_verticalInput;

	public void SetInput(float vertical, float horizontal, float brake)
	{
		m_horizontalInput = horizontal;
		m_verticalInput = vertical;
	}


	void Update()
	{
		// Get input axes
		float steer = m_horizontalInput;
		float gas = m_verticalInput;

		// Are they trying to move?
		if (gas != 0)
		{
			// Calculate the distance moved
			float moveDist = gas * speed * Time.deltaTime;
			// Calculate the turn angle
			float turnAngle = steer * turnSpeed * Time.deltaTime * gas;

			// Start the turn
			// Move in the correct direction
			transform.Translate(Vector3.forward * moveDist);
		}
	}
}