using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleARCore;

public class ARController : MonoBehaviour {

    //This list will be filled with the planes that ARCore detects in the current frame
    private List<TrackedPlane> m_NewTrackedPlanes = new List<TrackedPlane>();

    public GameObject GridPrefab;

    public GameObject portal;

    public GameObject ARCamera;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update() {
        //Checks ARCore status
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }
        //The following function will fill m_NewDetectedPlanes with the planes that ARCore detects in the current frame
        Session.GetTrackables<TrackedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);

        //Instantiate a Grid for each TrackedPlane in m_NewTrackedPlanes
        for (int i = 0; i < m_NewTrackedPlanes.Count; ++i)
        {
            GameObject grid = Instantiate(GridPrefab, Vector3.zero, Quaternion.identity, transform);

            //This function will set the position of the grid and modify the vertices of the attached mesh
            grid.GetComponent<GridVisualiser>().Initialize(m_NewTrackedPlanes[i]);
        }

        //Checks if the user touches the screen
        Touch touch;
        if(Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        //Checks if the user touched any of the tracked planes
        TrackableHit hit;
        if (Frame.Raycast(touch.position.x, touch.position.y, TrackableHitFlags.PlaneWithinPolygon, out hit))
        {
            //Places the portal on top of the tracked plane that was touched
            //Enables the portal
            portal.SetActive(true);

            //Create anew Anchor
            Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);

            //Set the position and rotation of the portal to be the same as the hit position
            portal.transform.position = hit.Pose.position;
            portal.transform.rotation = hit.Pose.rotation;

            //Makes the portal face the camera
            Vector3 cameraPosition = ARCamera.transform.position;

            //The portal should only rotate around the Y axis
            cameraPosition.y = hit.Pose.position.y;

            //Rotate the portal to face the camera
            portal.transform.LookAt(cameraPosition, portal.transform.up);

            //ARCore will keep understanding the world and update the anchors accordingly, hence we need to attach our portal to the anchor.
            portal.transform.parent = anchor.transform;
        }
    }
}
