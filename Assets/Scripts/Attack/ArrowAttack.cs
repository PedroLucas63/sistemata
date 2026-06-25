using System;
using Sistemata.Core;
using Sistemata.Player;
using Sistemata.Stats;
using Sistemata.Enemy;
using Sistemata.Ally;
using UnityEngine;
using Sistemata.Audio;

namespace Sistemata.Attack
{
    public class ArrowAttack : BaseAttack
    {
        [Header("Configurações da Flecha")]
        [SerializeField] private Projectile arrowPrefab;
        [SerializeField] private float arrowSpeed = 12f;
        [SerializeField] private float fanAngleSpread = 15f;
        [SerializeField] private AudioClip shootSound;

        private Ally.Ally _cachedAllyOwner;
        private EnemyController _cachedEnemyOwner;

        protected override void Start()
        {
            base.Start();

            _cachedAllyOwner = GetComponentInParent<Ally.Ally>();
            _cachedEnemyOwner = GetComponentInParent<EnemyController>();
        }

        protected override void ExecuteAttack()
        {
            if (!ProjectilePoolManager.Instance || !arrowPrefab) return;

            var amount = Mathf.Max(1, Mathf.FloorToInt(AttackStats.GetStat(StatType.Amount).Get()));
            var damage = Damage;
            var ricochet = AttackStats.GetStat(StatType.Ricochet).Get();
            var size = AttackStats.GetStat(StatType.AreaSize).Get();

            var baseDirection = GetOwnerForwardDirection();

            var targetTag = belongsToPlayer || _cachedAllyOwner ? "Enemy" : "Player";

            var startAngle = -((amount - 1) * fanAngleSpread) / 2f;

            for (var i = 0; i < amount; i++)
            {
                var currentAngle = startAngle + (i * fanAngleSpread);
                var spawnDirection = Quaternion.Euler(0, currentAngle, 0) * baseDirection;
                var spawnPosition = transform.position + 0.5f * baseDirection;

                var proj = ProjectilePoolManager.Instance.GetProjectile(arrowPrefab, spawnPosition, Quaternion.identity);

                if (proj)
                    proj.Setup(spawnDirection, arrowSpeed, damage, ricochet, size, targetTag);
            }

            if (shootSound != null)
            {
                if (belongsToPlayer)
                {
                    AudioManager.Instance.PlaySFX2D(shootSound);
                }
                else
                {
                    AudioManager.Instance.PlaySFX3D(shootSound, transform.position);
                }
            }
        }

        /// <summary>
        /// Calcula matematicamente a "frente" real para onde o dono está apontando no plano XZ.
        /// Se for o Player, mira usando a posição do mouse no mundo 3D.
        /// </summary>
        private Vector3 GetOwnerForwardDirection()
        {
            var forwardVector = transform.right; // Failsafe padrão

            if (belongsToPlayer && PlayerManager.Instance)
            {
                // Pega a câmera principal do jogo
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    // Cria um raio que sai da câmera e passa pela posição do mouse na tela
                    Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

                    // Cria um "chão invisível" perfeitamente plano na altura exata de onde a flecha vai nascer
                    Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

                    // Se o raio do mouse bater nesse chão...
                    if (groundPlane.Raycast(ray, out float enterDistance))
                    {
                        // Descobre a coordenada 3D exata desse ponto
                        Vector3 mouseWorldPosition = ray.GetPoint(enterDistance);

                        // A direção é o Ponto de Destino (Mouse) menos o Ponto de Origem (Player)
                        forwardVector = mouseWorldPosition - transform.position;
                    }
                }
            }
            else if (_cachedAllyOwner)
            {
                var allyLook = _cachedAllyOwner.LastMove;
                if (allyLook.sqrMagnitude > 0.001f)
                {
                    forwardVector = new Vector3(allyLook.x, 0f, allyLook.y);
                }
            }
            else if (_cachedEnemyOwner)
            {
                var target = _cachedEnemyOwner.Target;
                if (target != null)
                {
                    forwardVector = target.position - transform.position;
                }
                else
                {
                    forwardVector = transform.parent ? transform.parent.forward : transform.right;
                }
            }

            // Garante que a flecha não vai atirar para o céu nem para o subsolo
            forwardVector.y = 0;

            if (forwardVector.sqrMagnitude < 0.001f)
                forwardVector = transform.right;

            return forwardVector.normalized;
        }
    }
}