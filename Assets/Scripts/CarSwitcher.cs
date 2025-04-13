using System.Collections.Generic;
using UnityEngine;

public class CarSwitcher : MonoBehaviour
{
    [SerializeField] private List<CarData> cars;

    private int currentIndex = 0;
    private GameObject currentCarInstance;

    public void SwitchToNextCar()
    {
        if (cars.Count == 0) return;

        if (currentCarInstance != null)
        {
            Destroy(currentCarInstance);
        }

        currentIndex = (currentIndex + 1) % cars.Count;

        CarData selectedCar = cars[currentIndex];

        currentCarInstance = Instantiate(selectedCar.carView.carPrefab, transform.position, transform.rotation);
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SwitchToNextCar();
        }
    }
}
