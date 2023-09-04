using RedLoader;
using UnityEngine;
using UnityEngine.Serialization;

namespace SonsGameManager;

[RegisterTypeInIl2Cpp]
public class CustomFreeCam : MonoBehaviour
{
	private class CameraState
	{
		public float Yaw;

		public float Pitch;

		private float _roll;

		private float _x;

		private float _y;

		private float _z;

		public void SetFromTransform(Transform t)
		{
			var eulerAngles = t.eulerAngles;
			Pitch = eulerAngles.x;
			Yaw = eulerAngles.y;
			_roll = eulerAngles.z;
			
			var position = t.position;
			_x = position.x;
			_y = position.y;
			_z = position.z;
		}

		public void Translate(Vector3 translation)
		{
			Vector3 vector = Quaternion.Euler(Pitch, Yaw, _roll) * translation;
			_x += vector.x;
			_y += vector.y;
			_z += vector.z;
		}

		public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
		{
			Yaw = Mathf.Lerp(Yaw, target.Yaw, rotationLerpPct);
			Pitch = Mathf.Lerp(Pitch, target.Pitch, rotationLerpPct);
			_roll = Mathf.Lerp(_roll, target._roll, rotationLerpPct);
			_x = Mathf.Lerp(_x, target._x, positionLerpPct);
			_y = Mathf.Lerp(_y, target._y, positionLerpPct);
			_z = Mathf.Lerp(_z, target._z, positionLerpPct);
		}

		public void UpdateTransform(Transform t)
		{
			t.eulerAngles = new Vector3(Pitch, Yaw, _roll);
			t.position = new Vector3(_x, _y, _z);
		}
	}

	private CameraState _mTargetCameraState = new CameraState();

	private CameraState _mInterpolatingCameraState = new CameraState();

	public float _boost = 3.5f;
	public float _mouseBoost = 1f;

	public float _positionLerpTime;

	public AnimationCurve _mouseSensitivityCurve = new(new Keyframe(0f, 0f, 0f, 5f), new Keyframe(1f, 2f, 0f, 0f));

	public float _rotationLerpTime;

	public bool _invertY;

	private Rigidbody _rigidbody;
	
	private float _globalMouseSensitivity;
	private float _mouseYMultiplier;

	private void OnEnable()
	{
		_rigidbody = GetComponentInChildren<Rigidbody>();
		_rigidbody.isKinematic = true;

		_globalMouseSensitivity = Config.LookSensitivty.Value;
		_positionLerpTime = Config.PositionalSmoothing.Value;
		_rotationLerpTime = Config.RotationalSmoothing.Value;
		_mouseYMultiplier = Config.MouseYRatio.Value;

		ResetTransforms();
	}

	private void OnDisable()
	{
		_rigidbody.isKinematic = false;
	}

	public void ResetTransforms()
	{
		_mTargetCameraState.SetFromTransform(transform);
		_mInterpolatingCameraState.SetFromTransform(transform);
	}

	private Vector3 GetInputTranslationDirection()
	{
		Vector3 result = default(Vector3);
		if (Input.GetKey(KeyCode.W))
		{
			result += Vector3.forward;
		}
		if (Input.GetKey(KeyCode.S))
		{
			result += Vector3.back;
		}
		if (Input.GetKey(KeyCode.A))
		{
			result += Vector3.left;
		}
		if (Input.GetKey(KeyCode.D))
		{
			result += Vector3.right;
		}
		if (Input.GetMouseButton(1))
		{
			result += Vector3.down;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			result += Vector3.up;
		}
		return result;
	}

	private void Update()
	{
		Vector2 vector = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (_invertY ? 1 : (-1)));
		float num = _mouseSensitivityCurve.Evaluate(vector.magnitude) * _globalMouseSensitivity;
		_mTargetCameraState.Yaw += vector.x * num * _mouseBoost;
		_mTargetCameraState.Pitch += vector.y * num * _mouseYMultiplier * _mouseBoost;
		
		Vector3 translation = GetInputTranslationDirection() * Time.deltaTime;
		if (Input.GetKey(KeyCode.LeftShift))
		{
			translation *= 10f;
		}
		_boost += Input.mouseScrollDelta.y * 0.2f * (Input.GetKey(KeyCode.LeftControl) ? 0f : 1f);
		_mouseBoost += Input.mouseScrollDelta.y * 0.2f * (Input.GetKey(KeyCode.LeftControl) ? 1f : 0f);
		translation *= Mathf.Pow(2f, _boost);
		_mTargetCameraState.Translate(translation);
		float positionLerpPct = 1f - Mathf.Exp(Mathf.Log(0.00999999f) / _positionLerpTime * Time.deltaTime);
		float rotationLerpPct = 1f - Mathf.Exp(Mathf.Log(0.00999999f) / _rotationLerpTime * Time.deltaTime);
		_mInterpolatingCameraState.LerpTowards(_mTargetCameraState, positionLerpPct, rotationLerpPct);
		_mInterpolatingCameraState.UpdateTransform(transform);
	}
}