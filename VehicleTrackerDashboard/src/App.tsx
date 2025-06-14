import { useEffect, useState } from 'react'
import './index.css'
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";

// for datetime updates  
dayjs.extend(relativeTime);

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
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);
  const [sortBy, setSortBy] = useState('');




  useEffect(() => {
    setIsLoading(true); 
    fetchVehicles();
    const intervalId = setInterval(fetchVehicles, 5000); // Repeat every 5 seconds
    return () => clearInterval(intervalId); // Cleanup on unmount

  }, []);

  function fetchVehicles(){
    fetch("http://localhost:5067/api/vehicle/status")
      .then(res => res.json())
      .then(data => {
        console.log("Fetched vehicles:", data);
        setVehicles(data);
        setIsLoading(false); 
      })
      .catch(err => {
        console.error("Fetch error:", err);
        setError(true);       
        setIsLoading(false); 
      });
  }

  const sortedVehicles = [...vehicles].sort((a, b) => {
  if (sortBy === 'id') {
    return a.vehicleId - b.vehicleId;
  } else if (sortBy === 'speed') {
    return b.speed - a.speed; // descending
  } else if (sortBy === 'fuel') {
    return b.fuelLevel - a.fuelLevel; // descending
  }
  return 0;
});


  const getFuelColor = (level: number) => {
    if (level >= 50) return "bg-green-500";
    if (level >= 25) return "bg-yellow-500";
    return "bg-red-500";
  };




  return (
    <div className="min-h-screen bg-gray-900 text-white py-10 px-4">
      <div className="max-w-3xl mx-auto space-y-4">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-3xl font-bold mb-6">Vehicle Status</h1>
          <select
            className="bg-gray-800 text-white p-2 rounded mx-5"
            onChange={(e) => setSortBy(e.target.value)}
            >
              <option value="">Sort By</option>
              <option value="id">Vehicle ID</option>
              <option value="speed">Speed</option>
              <option value="fuel">Fuel Level</option>
            </select>
        </div>

        {isLoading && 
          <div className="flex justify-center">
            <div className="animate-spin rounded-full h-10 w-10 border-t-4 border-blue-500"></div>
          </div>
        }

        {error && <p className="text-red-500">Failed to load vehicle data.</p>}

        {!isLoading && !error && sortedVehicles.map((vehicle) => (
          <div
            key={vehicle.vehicleId}
            className="bg-gray-800 rounded-xl p-6 shadow shadow-gray-700 hover:shadow-2xl hover:shadow-black transition-shadow duration-300"
          >
            <div className="flex justify-between items-center mb-2">
              <h2 className="text-xl font-semibold">
                Vehicle #{vehicle.vehicleId}
              </h2>
              <span
                className={`px-5 py-1 ml-5 h rounded-full text-sm font-medium ${
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
            <div className="bg-gray-800 h-3 w-full">
                <div 
                  className={`h-full rounded ${getFuelColor(vehicle.fuelLevel)}`}
                  style={{width: `${vehicle.fuelLevel}%` }}
                >
                </div>
            </div>
            <p>
              Location: {vehicle.location.latitude.toFixed(3)},{" "}
              {vehicle.location.longitude.toFixed(3)}
            </p>
            <p className="text-sm text-gray-400 mt-2">
              Last updated: {dayjs(vehicle.timestamp).fromNow()}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
}
