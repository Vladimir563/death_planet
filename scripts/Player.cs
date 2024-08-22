using Godot;

public partial class Player : CharacterBody2D
{
	private const string PlayerAnimationNodeName = "PlayerAnimation2D";

	public const float Speed = 60.0f;

	public string CurrentDirection = MoveDirection.DOWN;

	private AnimatedSprite2D _animation;

	private bool _enemyInAttackRange = false;

	private bool _enemyAttackCooldown = true;

	private int _health = 100;

	private bool _playerAlive = true;

	private bool _attackIp = false;

    public override void _Ready()
    {
		_animation = GetNode(PlayerAnimationNodeName) as AnimatedSprite2D;
        _animation.Play(Animation.FRONT_IDLE);	
    }

    public override void _PhysicsProcess(double delta)
	{
		if(_health <= 0)
		{
			_playerAlive = false;
			_health = 0;
			_animation.Play("death_idle");
		}
		else
		{
			PlayerMovement(delta);
			EnemyAttack();
			Attack();
		}
	}

	public void PlayerMovement(double delta)
	{
		Vector2 velocity = default;
		if(Input.IsActionPressed("ui_right"))
		{
			velocity = GetPlayerMovement(MoveDirection.RIGHT, true, Speed, 0);
		}
		else if (Input.IsActionPressed("ui_left"))
		{
			velocity = GetPlayerMovement(MoveDirection.LEFT, true, -Speed, 0);
		}
		else if (Input.IsActionPressed("ui_down"))
		{
			velocity = GetPlayerMovement(MoveDirection.DOWN, true, 0, Speed);
		}
		else if (Input.IsActionPressed("ui_up"))
		{
			velocity = GetPlayerMovement(MoveDirection.UP, true, 0, -Speed);
		}
		else 
		{
			velocity = GetPlayerMovement(MoveDirection.NONE, false, 0, 0);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void OnDealAttackTimerTimeout()
	{
		var timer = GetNode<Timer>("deal_attack_timer") as Timer;
		timer.Stop();
		Global.PlayerCurrentAttack = false;
		_attackIp = false;
	}

	public void OnPlayerHitboxBodyEntered(Node2D body)
	{
		if(body.Name == "enemy" && body.HasMethod("Attack"))
		{
			_enemyInAttackRange = true;
		}
	}

	public void OnPlayerHitboxBodyExited(Node2D body)
	{
		if(body.Name == "enemy" && body.HasMethod("Attack"))
		{
			_enemyInAttackRange = false;
		}	
	}

	public void OnAttackCooldownTimeout()
	{
		_enemyAttackCooldown = true;
	}

	private void EnemyAttack()
	{
		if(_enemyInAttackRange && _enemyAttackCooldown && _playerAlive)
		{
			_health -= Global.EnemyAttackValue;
			_enemyAttackCooldown = false;
			var timer = GetNode<Timer>("attack_cooldown") as Timer;
			timer.Start(); 
			GD.Print(_health);
		}
	}

	private Vector2 GetPlayerMovement(string direction, bool isWalking, float xSpeed, float ySpeed)
	{
		Vector2 newVelocity = Velocity;
		if(direction != MoveDirection.NONE)
		{
			CurrentDirection = direction;
		}

		if(_attackIp == false)
		{
			PlayAnimation(isWalking);
		}

		newVelocity.X = xSpeed;
		newVelocity.Y = ySpeed;

		return newVelocity;
	}

	private void PlayAnimation(bool isWalking)
	{
		var anim = string.Empty;
		var direction = CurrentDirection;
		if(direction == MoveDirection.RIGHT)
		{
			_animation.FlipH = false;
			anim = isWalking ? Animation.SIDE_WALK : Animation.SIDE_IDLE;	
		}
		
		if(direction == MoveDirection.LEFT)
		{
			_animation.FlipH = true;
			anim = isWalking ? Animation.SIDE_WALK : Animation.SIDE_IDLE;
		}
		
		if(direction == MoveDirection.DOWN)
		{
			_animation.FlipH = true;
			anim = isWalking ? Animation.FRONT_WALK : Animation.FRONT_IDLE;
		}
		
		if(direction == MoveDirection.UP)
		{
			_animation.FlipH = true;
			anim = isWalking ? Animation.BACK_WALK : Animation.BACK_IDLE;
		}

		_animation.Play(anim);
	}

	private void Attack()
	{
		if(Input.IsActionJustPressed("attack"))
		{
			Global.PlayerCurrentAttack = true;
			_attackIp = true;	
			if(CurrentDirection == MoveDirection.RIGHT)
			{
				_animation.FlipH = false;
				_animation.Play("side_attack");
				var timer = GetNode<Timer>("deal_attack_timer") as Timer;		
				timer.Start();
			}
			else if(CurrentDirection == MoveDirection.LEFT)
			{
				_animation.FlipH = true;
				_animation.Play("side_attack");
				var timer = GetNode<Timer>("deal_attack_timer") as Timer;
				timer.Start();
			}
			else if(CurrentDirection == MoveDirection.DOWN)
			{
				_animation.Play("front_attack");
				var timer = GetNode<Timer>("deal_attack_timer") as Timer;
				timer.Start();
			}
			else if(CurrentDirection == MoveDirection.UP)
			{
				_animation.Play("back_attack");
				var timer = GetNode<Timer>("deal_attack_timer") as Timer;
				timer.Start();
			}		
		}
	}
}
