﻿using System;
using UnityEngine;

namespace UbiSolarSystem
{
    public class InputHandler : MonoBehaviour
    {
        /// <summary>
        /// Speed at which the planet will follow the mouse once dragged
        /// </summary>
        [Range(0f, 5f)]
        public float PlanetDragSpeed = 3f;
        /// <summary>
        /// Speed at which the camera will zoom back and forth
        /// </summary>
        public float ZoomSpeed = 30f;

        [HideInInspector()]
        public Planet SelectedPlanet;

        private Vector3 MapGrabPosition;
        private Vector3 InitialCameraPosition;

        private ClickContainer LeftClick;
        private ClickContainer RightClick;

        void Start()
        {
            LeftClick = new ClickContainer(ClickContainer.MOUSE_BUTTON.LEFT);
            RightClick = new ClickContainer(ClickContainer.MOUSE_BUTTON.RIGHT);

            LeftClick.OnClickEvent += SelectPlanet;
            LeftClick.OnDragEvent += DragPlanet;
            LeftClick.OnReleaseEvent += ReleasePlanet;

            RightClick.OnClickEvent += StartGrabMap;
            RightClick.OnDragEvent += DragMap;
        }

        void Update()
        {
            ProcessMouseButton(LeftClick);
            ProcessMouseButton(RightClick);

            transform.position = Vector3.Lerp(transform.position, transform.position + transform.forward * Input.mouseScrollDelta.y, ZoomSpeed * Time.deltaTime);
        }

        private void ProcessMouseButton(ClickContainer mouseClick)
        {
            // Clicked on this frame
            if (Input.GetMouseButtonDown((int)mouseClick.MouseButton) && mouseClick.ClickStatus == ClickContainer.CLICK_STATUS.HOVERING)
            {
                mouseClick.Click();
            }

            // Click held down
            if (Input.GetMouseButton((int)mouseClick.MouseButton) && (mouseClick.ClickStatus == ClickContainer.CLICK_STATUS.DRAGGING || mouseClick.ClickStatus == ClickContainer.CLICK_STATUS.CLICKING))
            {
                mouseClick.Drag();
            }

            // Click released
            if (Input.GetMouseButtonUp((int)mouseClick.MouseButton) && (mouseClick.ClickStatus == ClickContainer.CLICK_STATUS.DRAGGING || mouseClick.ClickStatus == ClickContainer.CLICK_STATUS.CLICKING))
            {
                mouseClick.Release();
            }

            // Not doing anything
            if (!Input.GetMouseButton((int)mouseClick.MouseButton) && mouseClick.ClickStatus == ClickContainer.CLICK_STATUS.RELEASING)
            {
                mouseClick.Hover();
            }
        }

        /// <summary>
        /// Tries to select a planet at mouse position
        /// </summary>
        private void SelectPlanet()
        {
            SelectedPlanet = GetPlanetAtPosition(Input.mousePosition);
        }

        /// <summary>
        /// Tries to drag the held planet
        /// </summary>
        private void DragPlanet()
        {
            if (SelectedPlanet != null)
            {
                Vector3 currentPos = SelectedPlanet.gameObject.transform.position;
                Vector3 interpolatedPos = Vector3.Lerp(SelectedPlanet.gameObject.transform.position, GetMousePositionInWorld(), Time.deltaTime * PlanetDragSpeed);
                // Calculate the acceleration to simulate momentum
                Vector3 acceleration = (interpolatedPos - currentPos) * (1 / Time.deltaTime);

                SelectedPlanet.Velocity = acceleration;
                SelectedPlanet.gameObject.transform.position = Vector3.Lerp(SelectedPlanet.gameObject.transform.position, GetMousePositionInWorld(), Time.deltaTime * PlanetDragSpeed);
                Cursor.visible = false;
            }
        }

        /// <summary>
        /// Tries to release the held planet
        /// </summary>
        private void ReleasePlanet()
        {
            Cursor.visible = true;
            SelectedPlanet = null;
        }

        /// <summary>
        /// Called when the user initiates a right click to move
        /// </summary>
        private void StartGrabMap()
        {
            MapGrabPosition = Input.mousePosition;
            InitialCameraPosition = transform.position;
        }

        /// <summary>
        /// Called when the user is dragging the map with right click
        /// </summary>
        private void DragMap()
        {
            Vector3 grabDirection = (Input.mousePosition - MapGrabPosition) / 10;
            grabDirection.z = grabDirection.y;
            grabDirection.y = 0;
            transform.position = InitialCameraPosition - grabDirection;
        }

        #region Helper functions
        /// <summary>
        /// Returns the planet at a given position if it exists. Null otherwise.
        /// </summary>
        /// <param name="position">The position to look for a planet</param>
        /// <returns>The planet at given position</returns>
        public static Planet GetPlanetAtPosition(Vector3 position)
        {
            RaycastHit hitInfo;
            Ray rayhit = Camera.main.ScreenPointToRay(position);

            // Raycast only on PLANET layer and ignore triggers
            Physics.Raycast(rayhit, out hitInfo, 1000, 1 << LayerManager.ToInt(LAYER.PLANET), QueryTriggerInteraction.Ignore);

            if (hitInfo.collider != null)
            {
                return hitInfo.collider.GetComponent<Planet>();
            }

            return null;
        }

        /// <summary>
        /// Returns the world position of the mouse along the (0,0,0) plane
        /// </summary>
        /// <returns>The mouse position in the world</returns>
        public static Vector3 GetMousePositionInWorld()
        {
            RaycastHit hitInfo;
            Ray rayhit = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Raycast only on the FLOOR layer and ignore triggers
            Physics.Raycast(rayhit, out hitInfo, 1000, 1 << LayerManager.ToInt(LAYER.FLOOR), QueryTriggerInteraction.Ignore);

            if (hitInfo.collider != null)
            {
                return hitInfo.point;
            }

            return Vector3.zero;
        }
        #endregion
    }
}