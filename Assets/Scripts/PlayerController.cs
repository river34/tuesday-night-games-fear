﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	// map
	private Map map;
	public bool map_up;
	public bool map_right;
	public bool map_bottom;
	public bool map_left;

	// strength
	public float strength;
	private int min_strength = 0;
	private int max_strength = 1000;
	private int strengthGain = 300;
	private int strengthLoss = 10;

	// fear
	public float fear;
	private int min_fear = 0;
	public int mid_fear = 500;
	private int max_fear = 1000;
	private int fearGain = 50;
	private int fearLoss = 10;
	private int courageGain = 300;
	private int courageLoss = 1;

	// position
	private Vector3 last_position;

	// input
	public float speed;
	public float jump_speed;
	public float walk_speed;
	public float run_speed;
	private float up_speed = 0;
	private float down_speed = 0;
	private float gravity = 10f;
	private bool is_moving;
	private bool is_running;
	private bool is_left;
	private bool is_up;
	private List<GameObject> monsters = new List<GameObject>();

	// quest
	private QuestManager questManager;

	// sprite
	private Transform sprite;
	private SpriteRenderer render;
	private Animator animator;
	private Color halfClear;

	// completes
	public GameObject complete;
	private List<GameObject> completes = new List<GameObject>();

	private float reverseMul = 0.001f;

	void Awake ()
	{
		sprite = transform.Find ("Sprite");
		animator = sprite.GetComponent <Animator>();
		render = sprite.GetComponent <SpriteRenderer>();
		halfClear = Color.yellow;
		halfClear.a = 0.2f;
	}

	void Start ()
	{
		strength = GameController.instance.playerStrength;
		fear = GameController.instance.playerFear;
		last_position = transform.position;
		render.color = Color.Lerp (halfClear, Color.yellow, strength * reverseMul);
		questManager = GameController.instance.questManager;
	}

	// Update is called once per frame
	void Update () {

		UpdateInput ();
		UpdateMap ();
		UpdateStatus ();
	}

	void LateUpdate ()
	{
		if (last_position != transform.position)
		{
			is_moving = true;
			animator.SetBool ("IsWalking", true);
			if (transform.position.x <= last_position.x)
			{
				is_left = true;
			}
			else
			{
				is_left = false;
			}
			if (transform.position.z >= last_position.z)
			{
				is_up = true;
			}
			else
			{
				is_up = false;
			}
		}
		else
		{
			is_moving = false;
			animator.SetBool ("IsWalking", false);
			is_left = false;
			is_up = false;
		}

		last_position = transform.position;

		if (is_moving && is_left)
		{
			sprite.localScale = new Vector3 (1, 1, 1);
		}
		else if (is_moving && !is_left)
		{
			sprite.localScale = new Vector3 (-1, 1, 1);
		}

		if (is_moving && is_up)
		{

		}
		else if (is_moving && !is_up)
		{

		}
	}

	// reach border of the current map
	void ReachBoarder ()	// up - 0, right - 1, bottom - 2, left - 3, none - -1
	{
		if (transform.position.z >= map.up - 20f)
		{
			map_up = true;
		}
		else
		{
			map_up = false;
		}
		if (transform.position.x >= map.right - 30f)
		{
			map_right = true;
		}
		else
		{
			map_right = false;
		}
		if (transform.position.z <= map.bottom + 20f)
		{
			map_bottom = true;
		}
		else
		{
			map_bottom = false;
		}
		if (transform.position.x <= map.left + 30f)
		{
			map_left = true;
		}
		else
		{
			map_left = false;
		}
	}

	void UpdateInput ()
	{
		if (Input.GetKey("w"))
		{
			transform.position += Vector3.forward * speed * Time.deltaTime;
		}
		else if (Input.GetKey("s"))
		{
			transform.position += Vector3.back * speed * Time.deltaTime;
		}

		if (Input.GetKey("a"))
		{
			transform.position += Vector3.left * speed * Time.deltaTime;
		}
		else if (Input.GetKey("d"))
		{
			transform.position += Vector3.right * speed * Time.deltaTime;
		}

		/*
		if (Input.GetKey("space"))
		{
			if (transform.position.y <= 0)
			{
				up_speed = jump_speed;
			}
		}
		*/

		if (Input.GetKey("left shift") || Input.GetKey("right shift") || Input.GetKey("space"))
		{
			speed = run_speed;
			is_running = true;
		}
		else
		{
			speed = walk_speed;
			is_running = false;
		}

		if (up_speed > 0)
		{
			up_speed -= gravity * Time.deltaTime;
			transform.position += Vector3.up * up_speed * Time.deltaTime;
		}
		else
		{
			up_speed = 0;
		}

		if (up_speed == 0 && transform.position.y > 0)
		{
			down_speed += gravity * Time.deltaTime;
			transform.position += Vector3.down * down_speed * Time.deltaTime;
		}

		if (down_speed > 0 && transform.position.y <= 0)
		{
			down_speed = 0;
			transform.position = new Vector3 (transform.position.x, 0, transform.position.z);
		}
	}

	void UpdateMap ()
	{
		if (map == null)
		{
			map = GameController.instance.FindMap (transform.position.x, transform.position.z);
		}

		if (map == null)
		{
			return;
		}

		ReachBoarder();

		if (map_up)
		{
			GameController.instance.GenerateMap (map.left + GameController.instance.map_width/2, map.up + GameController.instance.map_height/2);
		}
		if (map_right)
		{
			GameController.instance.GenerateMap (map.right + GameController.instance.map_width/2, map.bottom + GameController.instance.map_height/2);
		}
		if (map_bottom)
		{
			GameController.instance.GenerateMap (map.left + GameController.instance.map_width/2, map.bottom - GameController.instance.map_height/2);
		}
		if (map_left)
		{
			GameController.instance.GenerateMap (map.left - GameController.instance.map_width/2, map.bottom + GameController.instance.map_height/2);
		}

		if (map_up && map_right)
		{
			GameController.instance.GenerateMap (map.right + GameController.instance.map_width/2, map.up + GameController.instance.map_height/2);
		}
		if (map_right && map_bottom)
		{
			GameController.instance.GenerateMap (map.right + GameController.instance.map_width/2, map.bottom - GameController.instance.map_height/2);
		}
		if (map_bottom && map_left)
		{
			GameController.instance.GenerateMap (map.left - GameController.instance.map_width/2, map.bottom - GameController.instance.map_height/2);
		}
		if (map_left && map_up)
		{
			GameController.instance.GenerateMap (map.left - GameController.instance.map_width/2, map.up + GameController.instance.map_height/2);
		}

		if (transform.position.z > map.up || transform.position.z < map.bottom ||
			transform.position.x > map.right || transform.position.x < map.left)
		{
			map = GameController.instance.FindMap (transform.position.x, transform.position.z);
		}
	}

	void UpdateStatus ()
	{
		if (is_moving)
		{
			if (monsters.Count > 0 && fear > mid_fear)
			{
				LossStrength (strengthLoss * Time.deltaTime * 3);
			}
			else if (monsters.Count > 0 && fear > min_fear)
			{
				LossStrength (strengthLoss * Time.deltaTime * 2);
			}
			else
			{
				LossStrength (strengthLoss * Time.deltaTime);
			}
		}

		if (is_running)
		{
			GainFear (fearGain * Time.deltaTime);
		}
		else
		{
			if (monsters.Count > 0)
			{
				LossFear (fearLoss * Time.deltaTime);
			}
		}
	}

	public void GainStrength (int gain)
	{
		strength += gain;

		if (strength > max_strength)
		{
			strength = max_strength;
		}

		render.color = Color.Lerp (Color.yellow, halfClear, strength * reverseMul);

		GameController.instance.playerStrength = strength;
	}

	public void LossStrength (float loss)
	{
		if (fear >= max_fear)
		{
			strength -= loss * 2;
		}
		else
		{
			strength -= loss;
		}

		if (strength < min_strength)
		{
			strength = min_strength;
		}

		render.color = Color.Lerp (halfClear, Color.yellow, strength * reverseMul);

		GameController.instance.playerStrength = strength;
		CheckIfGameOver ();
	}

	public void GainCourage (float gain)
	{
		LossFear (gain);
	}

	public void GainFear (float gain)
	{
		fear += gain;
		if (fear > max_fear)
		{
			fear = max_fear;
		}
		GameController.instance.playerFear = fear;
	}

	public void LossFear (float loss)
	{
		fear -= loss;
		if (fear < min_fear)
		{
			fear = min_fear;
		}
		GameController.instance.playerFear = fear;
	}

	private void CheckIfGameOver ()
	{
		if (strength <= min_strength)
		{
			GameController.instance.GameOver ();
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.CompareTag ("Strength"))
		{
			GainStrength (strengthGain);
			Destroy (other.gameObject);
		}
		if (other.gameObject.CompareTag ("Courage"))
		{
			GainCourage (courageGain);
			Destroy (other.gameObject);
		}
		if (other.gameObject.CompareTag ("Monster"))
		{
			monsters.Add (other.gameObject);
		}
		if (other.gameObject.CompareTag ("WisdomTree"))
		{
			//
		}
		if (other.gameObject.CompareTag ("SpiritTree"))
		{
			//
		}

		if (questManager.quests.Count > 0)
		{
			foreach (Quest quest in questManager.quests)
			{
				if (quest.isFinished)
				{
					continue;
				}
				if (!quest.isOpen)
				{
					continue;
				}
				if (quest.tag != null && other.gameObject.CompareTag (quest.tag))
				{
					questManager.quests[quest.id].objects.Add (other.gameObject);
					bool completeNew = questManager.CheckForComplete (quest.id, this);
					if (completeNew)
					{
						GameObject completeObject = Instantiate (complete, transform, false);
						completeObject.AddComponent <CompleteController> ();
						completes.Add (completeObject);
					}
				}
			}
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (other.gameObject.CompareTag ("Monster"))
		{
			monsters.Remove (other.gameObject);
		}
	}
}
