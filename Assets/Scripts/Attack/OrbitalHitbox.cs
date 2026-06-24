using System;
using Sistemata.Enemy;
using UnityEngine;

namespace Sistemata.Attack
{
    public class OrbitalHitbox : MonoBehaviour
    {
        private OrbitalAttack _parent;
        private float _attackTimer;

        public void ConfigureAttack(OrbitalAttack parent)
        {
            _parent = parent;
            _attackTimer = 0;
        }

        private void Update()
        {
            _attackTimer -= Time.deltaTime;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (_attackTimer > 0) return;
            if (!other.CompareTag("Enemy")) return;
            
            ApplyAttack(other);
        }

        private void ApplyAttack(Collider other)
        {
            var enemy = other.GetComponentInParent<EnemyController>();
            if (!enemy) return;
            
            enemy.TakeDamage(_parent.Damage);
            _attackTimer = _parent.AttackTime;
        }
    }
}