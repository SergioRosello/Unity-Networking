using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Character : MonoBehaviour {
	public float MovementSpeed = 2;
    public string Name {
        get {
            return _name;
        }
        set {
            _name = value;
            gameObject.name = _name;
            if (_scoreText) {
                _scoreText.text = Name + ": " + Score;
            }
        }
    }

    public int Score {
        get {
            return _score;
        } set {
            _score = value;
            if (_scoreText) {
                _scoreText.text = Name + ": " + Score;
            }
        }
    }

	protected Rigidbody2D _rb;
	protected SpriteRenderer _sr;
	protected Health _health;
	protected Animator _anim;
    protected Text _scoreText;
    protected int _score;
    protected string _name;

	// Use this for initialization
	protected virtual void Awake () {
		_rb = GetComponent<Rigidbody2D> ();
		_sr = GetComponent<SpriteRenderer> ();
		_health = GetComponent<Health> ();
		_anim = GetComponent<Animator> ();
        _scoreText = GetComponentInChildren<Text>();
	}

    protected virtual void Start () {
        Score = 0;
    }
	
	// Update is called once per frame
	void Update () {
		if (!_health || _health.IsAlive) {
            ApplyMovement();
			_sr.sortingOrder = (int)(-transform.position.y * 100);
		} else {
			_rb.velocity = Vector2.zero;
		}
	}

	protected abstract Vector2 GetMovement ();

    public void SetPosition(Vector2 pos) {
        _rb.MovePosition(pos);
    }

    protected virtual void ApplyMovement() {
        var dir = GetMovement();
        _rb.velocity = dir * MovementSpeed;
    }

	void LateUpdate() {
		if (!_health || _health.IsAlive) {
			_anim.SetFloat ("xSpeed", Mathf.Abs (_rb.velocity.x));
			_anim.SetFloat ("ySpeed", _rb.velocity.y);
			_anim.SetFloat ("speed", Mathf.Clamp01 (_rb.velocity.magnitude));

			if (_rb.velocity.x > 0) {
				_sr.flipX = true;
			} else if (_rb.velocity.x < 0) {
				_sr.flipX = false;
			}

		} else {
			_anim.SetFloat ("speed", 1);
		}
	}

	virtual protected void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.layer == Layers.Pickable) {
            GameManager.Instance.PickedChest(this, other);
		}
	}
}
