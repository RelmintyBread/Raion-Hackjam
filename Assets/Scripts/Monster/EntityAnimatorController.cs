using UnityEngine;

/// <summary>
/// Pengontrol Animator berbasis arah gerak, TIDAK baca input sendiri.
/// Cocok dipakai AI/monster (atau player) asal script gerak yang lain
/// yang manggil UpdateAnimationDirection() tiap frame dengan arah geraknya.
///
/// Contoh dipanggil dari script AI:
///   Vector2 dir = (targetPosition - transform.position);
///   animController.UpdateAnimationDirection(dir, dir.magnitude > 0.05f);
/// </summary>
[RequireComponent(typeof(Animator))]
public class EntityAnimatorController : MonoBehaviour
{
    Animator animator;
    Vector2 lastMoveDirection = Vector2.down; // default menghadap bawah

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Panggil ini dari script gerak (AI atau player) tiap frame.
    /// </summary>
    /// <param name="rawDirection">Arah gerak (gak wajib dinormalisasi, misal velocity atau (target - posisi))</param>
    /// <param name="isMoving">True kalau entity sedang bergerak</param>
    public void UpdateAnimationDirection(Vector2 rawDirection, bool isMoving)
    {
        if (isMoving && rawDirection.sqrMagnitude > 0.0001f)
        {
            lastMoveDirection = rawDirection.normalized;
        }

        // Baik lagi jalan maupun diam, Horizontal/Vertical tetap ngirim arah terakhir,
        // supaya idle tetap menghadap ke arah yang benar (bukan reset ke tengah).
        animator.SetFloat("Horizontal", lastMoveDirection.x);
        animator.SetFloat("Vertical", lastMoveDirection.y);
        animator.SetFloat("Speed", isMoving ? 1f : 0f);
    }
}