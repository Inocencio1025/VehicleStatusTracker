import { useEffect, useState } from 'react'
import '../index.css'
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import * as signalR from "@microsoft/signalr";
import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';

// for datetime updates  
dayjs.extend(relativeTime);

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

type VehicleStatus = {
  vehicleId: number;
  make: string;
  model: string;
  year: number;
  speed: number;
  fuelLevel: number;
  engineHealth: string;
  timestamp: string;
  location: {
    latitude: number;
    longitude: number;
  };
};

type VehicleHistoryDto = {
  avgSpeed: number;
  maxSpeed: number;
  totalMileage: number;
  lastRefueled: string | null;
  history: VehicleStatus[];
};

export default function Dashboard() {

  const [statuses, setStatuses] = useState<VehicleStatus[]>([]);
  const [selectedVehicleId, setSelectedVehicleId] = useState<number | null>(null);
  const [selectedVehicle, setSelectedVehicle] = useState<VehicleHistoryDto | null>(null); const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);
  const [sortBy, setSortBy] = useState('');
  const [connectionStatus, setConnectionStatus] = useState<'connecting' | 'connected' | 'reconnecting' | 'disconnected'>('connecting');


  useEffect(() => {
    let isMounted = true;
    let connection: signalR.HubConnection | null = null;

    const setup = async () => {
      setIsLoading(true);
      await fetchStatuses(isMounted);

      connection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE_URL}/hubs/vehicle`, {
          accessTokenFactory: () => localStorage.getItem("token") ?? ""
        }).withAutomaticReconnect()
        .build();

      connection.onreconnecting(() => {
        if (!isMounted) return;
        setConnectionStatus('reconnecting');
      });

      connection.onreconnected(() => {
        if (!isMounted) return;
        setConnectionStatus('connected');
      });

      connection.onclose(() => {
        if (!isMounted) return;
        setConnectionStatus('disconnected');
      });


      connection.on("VehicleStatusUpdated", (updated: VehicleStatus) => {
        if (!isMounted) return;

        setStatuses(prev => {
          const idx = prev.findIndex(v => v.vehicleId === updated.vehicleId);

          if (idx === -1) return [...prev, updated];

          const copy = [...prev];
          copy[idx] = updated;
          return copy;
        });

      });

      try {
        await connection.start();
        if (isMounted) {
          setConnectionStatus('connected');
        }
      } catch (err) {
        console.error("SignalR connection error:", err);
        if (isMounted) {
          setConnectionStatus('disconnected');
        }
      }
    };

    setup();

    return () => {
      isMounted = false;
      connection?.stop();
    };

  }, []);

  useEffect(() => {
    if (selectedVehicleId === null) return;

    fetchVehicleHistory(selectedVehicleId);
  }, [selectedVehicleId]);

  async function fetchStatuses(isMounted = true) {
    const token = localStorage.getItem("token");
    try {
      const res = await fetch(`${API_BASE_URL}/api/vehicle/status`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });

      console.log("STATUS:", res.status);

      if (!res.ok) {
        throw new Error(`HTTP error ${res.status}`);
      }

      const data = await res.json();

      console.log("Fetched statuses:", data);

      if (!isMounted) return;

      setStatuses(data);
      setError(false);
    } catch (err) {
      console.error("Fetch error:", err);

      if (!isMounted) return;

      setError(true);
    } finally {
      if (isMounted) setIsLoading(false);
    }
  }

  async function fetchVehicleHistory(vehicleId: number) {
    const token = localStorage.getItem("token");

    try {
      const res = await fetch(
        `${API_BASE_URL}/api/vehicle/${vehicleId}/history?hours=24`,
        {
          headers: {
            Authorization: `Bearer ${token}`
          }
        }
      );

      if (!res.ok) throw new Error("Failed to fetch history");

      const data: VehicleHistoryDto = await res.json();

      setSelectedVehicle(data);
    } catch (err) {
      console.error(err);
      setSelectedVehicle(null);
    }
  }

  const sortedStatuses = [...statuses].sort((a, b) => {
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

  const connectionBadgeStyles: Record<typeof connectionStatus, string> = {
    connecting: "bg-blue-600",
    connected: "bg-green-600",
    reconnecting: "bg-yellow-500 text-black",
    disconnected: "bg-red-600"
  };




  return (
    <div className="min-h-screen bg-gray-900 text-white py-10 px-4">
      <div className="max-w-7xl mx-auto flex gap-6">

        {/* LEFT PANEL */}
        <div className="w-96 space-y-4">

          <div className="flex items-center justify-between mb-6">
            <div>
              <h1 className="text-3xl font-bold">Vehicle Status</h1>

              <span
                className={`inline-block mt-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wide ${connectionBadgeStyles[connectionStatus]}`}
              >
                {connectionStatus}
              </span>
            </div>

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

          {isLoading && (
            <div className="flex justify-center">
              <div className="animate-spin rounded-full h-10 w-10 border-t-4 border-blue-500" />
            </div>
          )}

          {error && (
            <p className="text-red-500">
              Failed to load vehicle data.
            </p>
          )}

          {!isLoading &&
            !error &&
            sortedStatuses.map((status) => (
              <div
                key={status.vehicleId}
                onClick={() => setSelectedVehicleId(status.vehicleId)}
                className="bg-gray-800 rounded-xl p-6 shadow shadow-gray-700 hover:shadow-2xl hover:shadow-black transition-shadow duration-300"
              >
                <div className="flex justify-between items-start mb-2 gap-4">
                  <div className="min-w-0">
                    <h2 className="text-xl font-semibold">
                      Vehicle #{status.vehicleId}
                    </h2>

                    <p className="text-sm text-gray-400">
                      {status.make} {status.model} • {status.year}
                    </p>
                  </div>

                  <span
                    className={`shrink-0 px-3 py-1 rounded-full text-sm font-medium ${status.engineHealth === "Good"
                      ? "bg-green-600"
                      : "bg-yellow-500"
                      }`}
                  >
                    {status.engineHealth}
                  </span>
                </div>

                <p>Speed: {status.speed} mph</p>

                <p>Fuel Level: {status.fuelLevel.toFixed(1)}%</p>

                <div className="bg-gray-700 h-3 w-full rounded">
                  <div
                    className={`h-full rounded ${getFuelColor(
                      status.fuelLevel
                    )}`}
                    style={{ width: `${status.fuelLevel}%` }}
                  />
                </div>

                <p>
                  Location: {status.location.latitude.toFixed(3)},
                  {" "}
                  {status.location.longitude.toFixed(3)}
                </p>

                <p className="text-sm text-gray-400 mt-2">
                  Last updated: {dayjs(status.timestamp).fromNow()}
                </p>
              </div>
            ))}
        </div>

        {/* RIGHT PANEL */}
        <div className="flex-1 bg-gray-800 rounded-xl p-6 flex flex-col gap-6">

          {/* Guard: nothing selected */}
          {!selectedVehicle ? (
            <div className="flex-1 flex items-center justify-center text-gray-400">
              Select a vehicle to view details
            </div>
          ) : (
            <>
              {/* TOP RIGHT: extra stats*/}
              <div className="flex-col gap-6">
                <h2 className="text-2xl font-bold">
                  {selectedVehicle.history[0].make}{" "}
                  {selectedVehicle.history[0].model}{" "}
                  {selectedVehicle.history[0].year}
                </h2>

                <p>Avg Speed: {selectedVehicle.avgSpeed}</p>
                <p>Max Speed: {selectedVehicle.maxSpeed}</p>
                <p>Total Mileage: {selectedVehicle.totalMileage}</p>
                <p>Last Refueled: {dayjs(selectedVehicle.lastRefueled).fromNow()}</p>
              </div>

              {/* BOTTOM: graph */}
              <div className="bg-gray-900 p-3 rounded-lg h-80 flex items-center justify-center text-gray-400">
                
                <h3 className="text-lg font-semibold mb-2">
                  Speed History
                </h3>

                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={selectedVehicle.history}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis
                      dataKey="timestamp"
                      reversed tickFormatter={(value) =>
                        new Date(value).toLocaleTimeString()
                      }
                    />                    
                    <YAxis dataKey="speed" />
                    <Tooltip />
                    <Line
                      type="monotone"
                      dataKey="speed"
                      dot={false}
                    />
                  </LineChart>
                </ResponsiveContainer>


              </div>
            </>
          )}

        </div>

      </div>
    </div>
  );
}