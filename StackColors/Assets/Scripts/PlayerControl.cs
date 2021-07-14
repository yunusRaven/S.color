using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private GameObject collected;
    [SerializeField] private settings settings;
    [SerializeField] private Color playerColor;
    public Color getPlayerColor { get { return playerColor; } }
    [SerializeField] private Renderer[] rends;
    [SerializeField] private bool isPlaying;
    Rigidbody rb;
    [SerializeField] private Transform parentPick;
    [SerializeField] private Transform StackPosition;
    [SerializeField] private float border;
    private bool atEnd;
    [SerializeField] private float forwardForce;
    public float getForwardForce { get { return forwardForce; } }
    [SerializeField] private float forceAdder;
    [SerializeField] private float forceReducer;
    int finishforce = 0;
    private int count;
    [SerializeField] private Camera orthoCamera;
    private Vector3 mousePos;
    private Vector3 mouseStartPos;
    private Vector3 movementDiff;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ColorSet(playerColor);
    }

    void Update()
    {
        mouseStartPos = Vector3.Lerp(mouseStartPos, mousePos, 0.1f);

        if (Input.GetMouseButtonDown(0))
        {
            MouseDown(Input.mousePosition);
        }

        else if (Input.GetMouseButton(0))
        {
            MouseHold(Input.mousePosition);
        }

        else if (Input.GetMouseButtonUp(0))
        {
            MouseUp();
        }

        if (isPlaying)
        {
            Move();

        }

        if (atEnd)
        {
            if (Input.GetMouseButtonDown(0))
            {
                finishforce = +1;
            }
            forwardForce += forceReducer * Time.deltaTime;
            if (forwardForce < 0)
            {
                forwardForce = 0;
            }
        }

    }

    void ColorSet(Color ColorIn)
    {
        playerColor = ColorIn;
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].material.SetColor("_Color", playerColor);
        }
    }


    void Move()
    {
        float xPosition = Mathf.Clamp(transform.position.x, border * -1f, border * 1f);
        transform.position = new Vector3(xPosition, transform.position.y, transform.position.z);


        float speed = settings.SideSpeed * Input.GetAxis("Horizontal");
        rb.velocity = new Vector3(movementDiff.x, 0, settings.ForwardSpeed);

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ColorWall"))
        {
            ColorSet(other.GetComponent<ColorWall>().GetColor(gameObject));
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishLineStart"))
        {
            atEnd = true;


        }
        if (other.CompareTag("FinishLineEnd"))
        {
            Kick();
            rb.velocity = Vector3.zero;
            isPlaying = false;
            atEnd = false;

        }
        if (atEnd)
        {
            return;
        }

        if (other.CompareTag("Collected"))
        {
            //Transform otherTransform = other.transform;

            if (playerColor == other.GetComponent<StackColors>().GetColor())
            {
                StackColors stackColor = other.GetComponent<StackColors>();
                Score.getInstance.UpdateScore(stackColor.getValue);

                other.transform.parent = parentPick;

                other.transform.localPosition = Vector3.up * count * 0.25f;

                count++;

                stackColor.GetStacked();
            }
            else
            {
                Score.getInstance.UpdateScore(other.GetComponent<StackColors>().getValue);
                Destroy(other.gameObject);

                if (parentPick != null)
                {
                    if (parentPick.childCount > 0)
                    {
                        count--;
                        count = Mathf.Clamp(count, 0, 100);

                        //parentPick.position += Vector3.down;
                        Destroy(parentPick.GetChild(parentPick.childCount - 1).gameObject);

                    }
                    //else
                    //{
                    //    Destroy(parentPick.gameObject);
                    //}
                }

                return;
            }


        }
    }
    private void MouseDown(Vector3 inputPos)
    {
        mousePos = orthoCamera.ScreenToWorldPoint(inputPos);
        mouseStartPos = mousePos;
    }
    private void MouseHold(Vector3 inputPos)
    {
        mousePos = orthoCamera.ScreenToWorldPoint(inputPos);
        movementDiff = mousePos - mouseStartPos;
        movementDiff *= settings.sensitivity;
    }
    private void MouseUp()
    {
        movementDiff = Vector3.zero;
    }


    void Kick()
    {
        Transform stackParent = parentPick.transform;

        for (int i = 0; i < stackParent.childCount; i++)
        {

            Rigidbody child = stackParent.GetChild(i).GetComponent<Rigidbody>();

            child.isKinematic = false;
            child.AddForce((Vector3.one - Vector3.right) * finishforce * (i + 3), ForceMode.Impulse);
        }
    }


}
