using UnityEngine;

namespace Sistemata.Common
{
    /// <summary>
    /// Script simples para ser colocado no objeto FILHO que contém o Animator.
    /// Ele repassa os eventos de animação para o script no objeto PAI.
    /// </summary>
    public class AnimationEventBridge : MonoBehaviour
    {
        // Cache do objeto pai para evitar busca em cada frame
        private GameObject _parent;

        private void Awake()
        {
            if (transform.parent != null)
            {
                _parent = transform.parent.gameObject;
            }
        }

        /// <summary>
        /// Este método deve ter o MESMO NOME do evento que você criou na linha do tempo da animação.
        /// </summary>
        public void OnAnimationAttackEvent()
        {
            if (_parent == null) return;

            // Tenta enviar a mensagem para o pai. 
            // O SendMessage chamará 'OnAnimationAttackEvent' em qualquer script que o possua no pai.
            _parent.SendMessage("OnAnimationAttackEvent", SendMessageOptions.DontRequireReceiver);
        }
    }
}
