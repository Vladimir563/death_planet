using System;
using System.Collections.Generic;
using Godot;

public partial class Enemy : CharacterBody2D
{
    public const float Speed = 45.0f;

    private bool _playerChase = false;

    private Node2D _player = null;

    private AnimatedSprite2D _enemyAnimation;

    private bool _stopMovement = false;

    private const string EnemyAnimationNodeName = "EnemyAnimation2D";

    private string _currentDirection = MoveDirection.DOWN;

    private bool _playerInAttackZone = false;

    private int _health = 100;

    private Dictionary<string, string> _idlesDict = new ()
    {
        { MoveDirection.RIGHT, Animation.SIDE_IDLE },
        { MoveDirection.LEFT, Animation.SIDE_IDLE },
        { MoveDirection.DOWN, Animation.FRONT_IDLE },
        { MoveDirection.UP, Animation.BACK_IDLE },  
    };

    public override void _Ready()
    {
		_enemyAnimation = GetNode(EnemyAnimationNodeName) as AnimatedSprite2D;
        _enemyAnimation.Play(Animation.FRONT_IDLE);
    }

    public override void _PhysicsProcess(double delta)
    {
        DealWithAttack();

        if(_health <= 0)
        {
            return;
        }

        if(_playerChase && !_stopMovement)
        {
            Vector2 direction = (_player.Position - Position).Normalized();
            Position += direction * Speed * (float)delta;
            if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
            {
                // Движение влево или вправо
                _enemyAnimation.Play(Animation.SIDE_WALK);
                _enemyAnimation.FlipH = direction.X < 0;

                _currentDirection = _enemyAnimation.FlipH ? MoveDirection.RIGHT : MoveDirection.LEFT;
            }
            else
            {
                if (direction.Y < 0)
                {
                    // Движение вверх
                    _enemyAnimation.Play(Animation.BACK_WALK);
                    _currentDirection = MoveDirection.UP;
                }
                else
                {
                    // Движение вниз
                    _enemyAnimation.Play(Animation.FRONT_WALK);
                    _currentDirection = MoveDirection.DOWN;
                }
            }

            MoveAndSlide();
        }
        else
        {
            _enemyAnimation.Play(_idlesDict[_currentDirection]);
        }
    }

    public void OnDetectionAreaBodyEntered(Node2D body)
    {
        MakeOnPlayerAction(body, () =>
        {
            _player = body;
            _playerChase = true;
        });
    }

    public void OnDetectionAreaBodyExited(Node2D body)
    {
        MakeOnPlayerAction(body, () =>
        {
            _player = null;
            _playerChase = false;
        });
    }

    public void  OnStopAreaBodyEntered(Node2D body)
    {
        MakeOnPlayerAction(body, () => {_stopMovement = true;});      
    }

    public void OnStopAreaBodyExited(Node2D body)
    {
        MakeOnPlayerAction(body, () => {_stopMovement = false;});
    }

    public void MakeOnPlayerAction(Node2D body, Action action)
    {
        if(string.Equals(body.Name, nameof(Player), StringComparison.OrdinalIgnoreCase))
        {
            action();
        }
    }

    public void OnEnemyHitboxBodyEntered(Node2D body)
    {
        if(body.Name == "player")
        {
            _playerInAttackZone = true;
        }
    }

    public void OnEnemyHitboxBodyExited(Node2D body)
    {
        if(body.Name == "player")
        {
            _playerInAttackZone = false;
        }
    }

    public void Attack()
    {
        GD.Print("Attacking!");
    }

    public void OnDeathTimerTimeout()
    {
        var deathTimer = GetNode<Timer>("death_timer") as Timer;
        deathTimer.Stop();
        this.QueueFree();
    }

    private void DealWithAttack()
    {
        if(_playerInAttackZone && Global.PlayerCurrentAttack && _health > 0)
        {
            _health -= Global.PlayerAttackValue;
            GD.Print($"Slime health: " + _health);
            if(_health <= 0)
            {
                var deathTimer = GetNode<Timer>("death_timer") as Timer;
                _enemyAnimation.Play("death"); 
                deathTimer.Start();                 
            }

            Global.PlayerCurrentAttack = false;
        }
    }
}


