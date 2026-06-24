using System.Collections.Generic;
using UnityEngine;
using Sistemata.Stats;

namespace Sistemata.Attack
{
    public class OrbitalAttack : BaseAttack
    {
        [Header("Configurações Orbitais")]
        [SerializeField] private OrbitalHitbox hitboxPrefab;
        
        [Tooltip("Velocidade com que as espadas giram (graus por segundo). Pode ser positivo ou negativo.")]
        [SerializeField] private float orbitSpeed = 180f;
        
        [Tooltip("Como o seu sprite tem a ponta no superior-esquerdo, esse valor calibra a rotação para a espada 'cortar' para a frente.")]
        [SerializeField] private float spriteRotationOffset = 45f;
        
        [Tooltip("Tomba a espada no eixo X para casar com a perspectiva 2.5D (mesmo usado no seu Projectile).")]
        [SerializeField] private float perspectiveXTilt = 45f;

        private readonly List<OrbitalHitbox> _activeHitboxes = new List<OrbitalHitbox>();
        private float _currentOrbitAngle = 0f;

        private float Radius => AreaSize;

        protected override void Start()
        {
            base.Start();
            SyncObjectsCount();
        }
        
        protected override void Update()
        {
            if (AmountInt != _activeHitboxes.Count)
                SyncObjectsCount();

            UpdateOrbit();
        }

        private void SyncObjectsCount()
        {
            var targetCount = AmountInt;

            while (_activeHitboxes.Count < targetCount)
            {
                var obj = Instantiate(hitboxPrefab, transform.position, Quaternion.identity, transform);
                obj.ConfigureAttack(this);
                _activeHitboxes.Add(obj);
            }

            while (_activeHitboxes.Count > targetCount && _activeHitboxes.Count > 0)
            {
                var objToRemove = _activeHitboxes[^1];
                _activeHitboxes.RemoveAt(_activeHitboxes.Count - 1);
                Destroy(objToRemove.gameObject);
            }
        }

        private void UpdateOrbit()
        {
            if (_activeHitboxes.Count == 0) return;

            _currentOrbitAngle += orbitSpeed * Time.deltaTime;
            if (_currentOrbitAngle >= 360f) _currentOrbitAngle -= 360f;

            var currentRadius = Radius;
            
            var angleStep = 360f / _activeHitboxes.Count;

            for (var i = 0; i < _activeHitboxes.Count; i++)
            {
                var angleDegrees = _currentOrbitAngle + (i * angleStep);
                var angleRadians = angleDegrees * Mathf.Deg2Rad;

                var x = Mathf.Cos(angleRadians) * currentRadius;
                var z = Mathf.Sin(angleRadians) * currentRadius;

                _activeHitboxes[i].transform.localPosition = new Vector3(x, 0f, z);

                _activeHitboxes[i].transform.localRotation = Quaternion.Euler(
                    0f, -angleDegrees + spriteRotationOffset, 0f
                );
            }
        }

        protected override void ExecuteAttack()
        {
            // Fica vazio propositalmente.
            // Como este ataque é contínuo, a lógica de dano é gerenciada 
            // 100% pelo OnTriggerEnter da sua OrbitalHitbox.
        }
    }
}