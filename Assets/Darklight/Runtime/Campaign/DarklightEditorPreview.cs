using UnityEngine;
namespace Darklight { public sealed class DarklightEditorPreview : MonoBehaviour {void Awake(){gameObject.SetActive(false);Destroy(gameObject);} } }
