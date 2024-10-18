using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public class ConstraintToWeapon : MonoBehaviour
{
    public static ConstraintToWeapon Instance { get; set; }

    [Header("Right Hand")]
    public GameObject[] targetsForRightHandFingers;
    public GameObject[] rigsForRightHand;

    [Header("Left Hand")]
    public GameObject[] targetsForLeftHandFingers;
    public GameObject[] rigsForLeftHand;

    [Header("Targets For Hands")]
    public GameObject targetRightHandToWeapon;
    public GameObject targetLeftHandToWeapon;

    [Header("Weapon Camera")]
    public GameObject cameraForWeapons;
    public Vector3 defaultOffsetForWeaponCameraPosition;
    public Vector3 defaultOffsetForWeaponCameraRotation;

    [Header("Right And Left Hands")]
    public GameObject rightHand;
    public GameObject leftHand;

    private Coroutine currentRightHandTransition = null;
    private Coroutine currentLeftHandTransition = null;
    private bool isInADS = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Активируем слежку оружия за правой рукой + активируем пальцы для хвата оружия
    public void AddWeaponIntoHands(GameObject pickedupWeapon)
    {
        ParentConstraint weaponConstraint = pickedupWeapon.GetComponent<ParentConstraint>();
        weaponConstraint.enabled = true;

        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = targetRightHandToWeapon.transform,
            weight = 1.0f
        };
        weaponConstraint.SetSources(new List<ConstraintSource> { source });
        weaponConstraint.constraintActive = true;

        for (int i = 0; i < targetsForRightHandFingers.Length; i++)
        {
            GameObject targetForFinger = targetsForRightHandFingers[i];
            GameObject targetForTargetForFinger = pickedupWeapon.GetComponent<Weapon>().targetsForTargetsForRightFingers[i];

            ActivateFingerConstraintToWeapon(targetForFinger, targetForTargetForFinger);
        }
    }

    // Активируем пальцы рук
    public void ActivateFingerConstraintToWeapon(GameObject targetForFinger, GameObject targetForTargetForFinger)
    {
        ParentConstraint fingerConstraint = targetForFinger.GetComponent<ParentConstraint>();
        fingerConstraint.enabled = true;

        ConstraintSource sourceForFinger1 = new ConstraintSource
        {
            sourceTransform = targetForTargetForFinger.transform,
            weight = 1.0f
        };
        fingerConstraint.SetSources(new List<ConstraintSource> { sourceForFinger1 });
        fingerConstraint.constraintActive = true;
    }

    // Деактивируем пальцы рук
    public void DeactivateFingerConstraintToWeapon(GameObject targetForFinger, GameObject targetForTargetForFinger)
    {
        ParentConstraint fingerConstraint = targetForFinger.GetComponent<ParentConstraint>();

        ConstraintSource sourceForFinger1 = new ConstraintSource
        {
            sourceTransform = null,
            weight = 0f
        };
        fingerConstraint.SetSources(new List<ConstraintSource> { sourceForFinger1 });
        fingerConstraint.constraintActive = false;
        fingerConstraint.enabled = false;
    }

    // Выставляем констреинты для камеры, правой и левой рук
    public void ActivateConstraint(GameObject player, GameObject weapon)
    {
        Weapon weaponScript = weapon.GetComponent<Weapon>();

        // Офсет камеры для оружия
        ParentConstraint cameraForWeaponConstraint = cameraForWeapons.GetComponent<ParentConstraint>();
        cameraForWeaponConstraint.SetTranslationOffset(0, weaponScript.cameraOffsetPosition);
        cameraForWeaponConstraint.SetRotationOffset(0, weaponScript.cameraOffsetRotation);

        // Правая рука
        ParentConstraint targetRightHandToWeaponConstraint = targetRightHandToWeapon.GetComponent<ParentConstraint>();
        targetRightHandToWeaponConstraint.SetTranslationOffset(0, weaponScript.spawnPositionInRightHand);
        targetRightHandToWeaponConstraint.SetRotationOffset(0, weaponScript.spawnRotationInRightHand);

        foreach (GameObject rig in rigsForRightHand)
        {
            print(rig.gameObject.name);
            if (rig.gameObject.name != "IKConstraintHeadToWeapon")
            {
                rig.gameObject.GetComponent<TwoBoneIKConstraint>().weight = 1f;
            }
        }

        //// Левая рука (Пока закомментирую, так как вполне возможно при нормальных анимациях мне не придется самостоятельно выставлять положение
        /// левой руки, если она не активна(она будет просто анимироваться обычной анимацией))

        ParentConstraint targetLeftHandToWeaponConstraint = targetLeftHandToWeapon.GetComponent<ParentConstraint>();
        targetLeftHandToWeaponConstraint.constraintActive = true;
        targetLeftHandToWeaponConstraint.SetTranslationOffset(0, weaponScript.spawnPositionInLeftHand);
        targetLeftHandToWeaponConstraint.SetRotationOffset(0, weaponScript.spawnRotationInLeftHand);

        //Устанавливаем вес для всех ригов левой руки
        foreach (var rig in rigsForLeftHand)
        {
            var leftHandChainConstraint = rig.GetComponent<ChainIKConstraint>();
            if (leftHandChainConstraint != null)
            {
                leftHandChainConstraint.weight = 1f;
            }

            var leftHandConstraint = rig.GetComponent<TwoBoneIKConstraint>();
            if (leftHandConstraint != null)
            {
                leftHandConstraint.weight = 1f;
            }
        }

        // Активируем веса для пальцев левой руки
        for (int i = 0; i < targetsForRightHandFingers.Length; i++)
        {
            GameObject targetForFinger = targetsForLeftHandFingers[i];
            GameObject targetForTargetForFinger = weapon.GetComponent<Weapon>().targetsForTargetsForLeftFingers[i];

            ActivateFingerConstraintToWeapon(targetForFinger, targetForTargetForFinger);
        }

        // Активируем констреинт для право и левой рук, чтобы они не отставали от оружия
        rightHand.GetComponent<ParentConstraint>().constraintActive = true;
        leftHand.GetComponent<ParentConstraint>().constraintActive = true;
    }

    // Выставляем дефолтные параметры офсета для камеры, правой и левой рук(когда нет оружия в руке)
    public void DeactivateConstraint(GameObject player, GameObject weapon)
    {
        Weapon weaponScript = weapon.GetComponent<Weapon>();

        // Офсет камеры для оружия
        ParentConstraint cameraForWeaponConstraint = cameraForWeapons.GetComponent<ParentConstraint>();
        cameraForWeaponConstraint.SetTranslationOffset(0, defaultOffsetForWeaponCameraPosition);
        cameraForWeaponConstraint.SetRotationOffset(0, defaultOffsetForWeaponCameraRotation);

        // Левая рука
        ParentConstraint targetLeftHandToWeaponConstraint = targetLeftHandToWeapon.GetComponent<ParentConstraint>();
        targetLeftHandToWeaponConstraint.constraintActive = false;

        //Устанавливаем вес для всех ригов левой руки
        foreach (var rig in rigsForLeftHand)
        {
            var leftHandChainConstraint = rig.GetComponent<ChainIKConstraint>();
            if (leftHandChainConstraint != null)
            {
                leftHandChainConstraint.weight = 0f;
            }

            var leftHandConstraint = rig.GetComponent<TwoBoneIKConstraint>();
            if (leftHandConstraint != null)
            {
                leftHandConstraint.weight = 0f;
            }
        }

        // Активируем веса для пальцев левой руки
        for (int i = 0; i < targetsForRightHandFingers.Length; i++)
        {
            GameObject targetForFinger = targetsForLeftHandFingers[i];
            GameObject targetForTargetForFinger = weapon.GetComponent<Weapon>().targetsForTargetsForLeftFingers[i];

            DeactivateFingerConstraintToWeapon(targetForFinger, targetForTargetForFinger);
        }


        // Правая рука
        foreach (GameObject rig in rigsForRightHand)
        {
            if (rig.gameObject.name != "IKConstraintHeadToWeapon")
            {
                rig.gameObject.GetComponent<TwoBoneIKConstraint>().weight = 0f;
            }
        }

        // Деактивируем констреинт для право и левой рук, чтобы они не отставали от оружия
        rightHand.GetComponent<ParentConstraint>().constraintActive = false;
        leftHand.GetComponent<ParentConstraint>().constraintActive = false;
    }

    // Вход в прицел
    public IEnumerator EnterADS(Vector3 spawnPositionInRightHandADS, Vector3 spawnRotationInRightHandADS, Vector3 spawnPositionInLeftHandADS, Vector3 spawnRotationInLeftHandADS, float ADSSpeed, bool isADS, GameObject weapon)
    {
        // Выставляем левую руку
        ParentConstraint targetLeftHandToWeaponConstraint = targetLeftHandToWeapon.GetComponent<ParentConstraint>();
        targetLeftHandToWeaponConstraint.constraintActive = true;

        isInADS = isADS;

        // Останавливаем корутины перехода, если они запущены
        if (currentRightHandTransition != null)
        {
            StopCoroutine(currentRightHandTransition);
        }
        if (currentLeftHandTransition != null)
        {
            StopCoroutine(currentLeftHandTransition);
        }

        // Позиция и ротация правой руки
        Vector3 currentRightHandPosition = targetRightHandToWeapon.GetComponent<ParentConstraint>().GetTranslationOffset(0);
        Vector3 currentRightHandRotation = targetRightHandToWeapon.GetComponent<ParentConstraint>().GetRotationOffset(0);

        // Позиция и ротация левой руки
        Vector3 currentLeftHandPosition = targetLeftHandToWeaponConstraint.GetTranslationOffset(0);
        Vector3 currentLeftHandRotation = targetLeftHandToWeaponConstraint.GetRotationOffset(0);

        // Запускаем корутины плавного перехода для обеих рук
        currentRightHandTransition = StartCoroutine(SmoothTransition(targetRightHandToWeapon.GetComponent<ParentConstraint>(), currentRightHandPosition, spawnPositionInRightHandADS, currentRightHandRotation, spawnRotationInRightHandADS, ADSSpeed));
        currentLeftHandTransition = StartCoroutine(SmoothTransition(targetLeftHandToWeaponConstraint, currentLeftHandPosition, spawnPositionInLeftHandADS, currentLeftHandRotation, spawnRotationInLeftHandADS, ADSSpeed));

        // Ждем завершения обоих переходов
        yield return currentRightHandTransition;
        yield return currentLeftHandTransition;
    }

    // Выход из прицела
    public void ExitADS(Vector3 spawnPositionInRightHand, Vector3 spawnRotationInRightHand, Vector3 spawnPositionInLeftHand, Vector3 spawnRotationInLeftHand, float ADSSpeed, bool isADS, GameObject weapon)
    {
        ParentConstraint targetRightHandToWeaponConstraint = targetRightHandToWeapon.GetComponent<ParentConstraint>();

        // Деактивируем левую руку
        ParentConstraint targetLeftHandToWeaponConstraint = targetLeftHandToWeapon.GetComponent<ParentConstraint>();

        isInADS = isADS;

        // Останавливаем корутины перехода, если они запущены
        if (currentRightHandTransition != null)
        {
            StopCoroutine(currentRightHandTransition);
        }
        if (currentLeftHandTransition != null)
        {
            StopCoroutine(currentLeftHandTransition);
        }

        // Позиция и ротация правой руки
        Vector3 currentRightHandPosition = targetRightHandToWeaponConstraint.GetTranslationOffset(0);
        Vector3 currentRightHandRotation = targetRightHandToWeaponConstraint.GetRotationOffset(0);

        // Позиция и ротация левой руки
        Vector3 currentLeftHandPosition = targetLeftHandToWeaponConstraint.GetTranslationOffset(0);
        Vector3 currentLeftHandRotation = targetLeftHandToWeaponConstraint.GetRotationOffset(0);

        // Запускаем корутины плавного перехода для обеих рук
        currentRightHandTransition = StartCoroutine(SmoothTransition(targetRightHandToWeaponConstraint, currentRightHandPosition, spawnPositionInRightHand, currentRightHandRotation, spawnRotationInRightHand, ADSSpeed));
        currentLeftHandTransition = StartCoroutine(SmoothTransition(targetLeftHandToWeaponConstraint, currentLeftHandPosition, spawnPositionInLeftHand, currentLeftHandRotation, spawnRotationInLeftHand, ADSSpeed));
    }

    // Плавное смещение
    private IEnumerator SmoothTransition(ParentConstraint constraint, Vector3 startPosition, Vector3 targetPosition, Vector3 startRotation, Vector3 targetRotation, float ADSSpeed)
    {
        float transitionProgress = 0f;
        float transitionSpeed = ADSSpeed > 0 ? 1f / ADSSpeed : 0.001f;

        // Плавный переход
        while (transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime / transitionSpeed;

            // Линейная интерполяция позиции и ротации
            Vector3 currentPositionOffset = Vector3.Lerp(startPosition, targetPosition, transitionProgress);
            Vector3 currentRotationOffset = Vector3.Lerp(startRotation, targetRotation, transitionProgress);

            // Применяем новые значения к Parent Constraint
            constraint.SetTranslationOffset(0, currentPositionOffset);
            constraint.SetRotationOffset(0, currentRotationOffset);

            yield return null; // Ждем до следующего кадра
        }

        // Обеспечиваем точное завершение перехода
        constraint.SetTranslationOffset(0, targetPosition);
        constraint.SetRotationOffset(0, targetRotation);

        // Сбрасываем текущую корутину
        if (constraint == targetRightHandToWeapon.GetComponent<ParentConstraint>())
        {
            currentRightHandTransition = null; // Переход завершен для правой руки
        }
        else if (constraint == targetLeftHandToWeapon.GetComponent<ParentConstraint>())
        {
            currentLeftHandTransition = null; // Переход завершен для левой руки
        }
    }

}
