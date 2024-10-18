using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowForTarget : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 100f;  // ���������������� �������� ����
    [SerializeField] private float verticalLimit = 30f;  // ����� ���� �� ��������� (� ��������)

    private float _rotationX = 0f;
    private float _rotationY = 0f;

    void Update()
    {
        // ��������� �������� ����
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;

        // ���������� ��������� �� �����������
        _rotationY += mouseX;

        // ���������� ��������� �� ��������� � ������������
        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, -verticalLimit, verticalLimit);

        // ���������� ��������
        transform.rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
    }
}
