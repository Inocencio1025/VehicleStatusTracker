import { useEffect, useState } from 'react'
import './index.css'

type Vehicle = {
  vehicleId: number;
  speed: number;
  fuelLevel: number;
  engineHealth: string;
  timestamp: string;
  location: {
    latitude: number;
    longitude: number;
  };
};

export default function App() {

  const [vehicles, setVehicles] = useState<Vehicle[]>([]);

useEffect(() => {
  fetch("http://localhost:5067/api/vehicle/status")
    .then(res => res.json())
    .then(data => {
      console.log("Fetched vehicles:", data);
      setVehicles(data);
    })
    .catch(err => console.error("Fetch error:", err));
}, []);


  return (
    <div className="min-h-screen bg-gray-900 text-white py-10 px-4">
      <div className="max-w-3xl mx-auto space-y-4">
        <h1 className="text-3xl font-bold mb-6">Vehicle Status</h1>

        {vehicles.map((vehicle) => (
          <div
            key={vehicle.vehicleId}
            className="bg-gray-800 rounded-xl p-6 shadow-md"
          >
            <div className="flex justify-between items-center mb-2">
              <h2 className="text-xl font-semibold">
                Vehicle #{vehicle.vehicleId}
              </h2>
              <span
                className={`px-3 py-1 rounded-full text-sm font-medium ${
                  vehicle.engineHealth === "Good"
                    ? "bg-green-600"
                    : "bg-yellow-500"
                }`}
              >
                {vehicle.engineHealth}
              </span>
            </div>

            <p>Speed: {vehicle.speed} mph</p>
            <p>Fuel Level: {vehicle.fuelLevel.toFixed(1)}%</p>
            <p>
              Location: {vehicle.location.latitude.toFixed(3)},{" "}
              {vehicle.location.longitude.toFixed(3)}
            </p>
            <p className="text-sm text-gray-400 mt-2">
              Last updated: {new Date(vehicle.timestamp).toLocaleString()}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
}
