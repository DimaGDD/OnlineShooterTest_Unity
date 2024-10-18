using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowForTarget : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 100f;  // Чувствительность движения мыши
    [SerializeField] private float verticalLimit = 30f;  // Лимит угла по вертикали (в градусах)

    private float _rotationX = 0f;
    private float _rotationY = 0f;

    void Update()
    {
        // Получение движения мыши
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;

        // Управление вращением по горизонтали
        _rotationY += mouseX;

        // Управление вращением по вертикали с ограничением
        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, -verticalLimit, verticalLimit);

        // Применение вращений
        transform.rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
    }
}
