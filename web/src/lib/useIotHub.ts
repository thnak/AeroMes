import { useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

export interface MachineSignalUpdate {
  machineCode: string;
  tagKey: string;
  value: number;
  unit: string | null;
  timestamp: string;
  isBadQuality: boolean;
}

export interface MachineStateUpdate {
  machineCode: string;
  newState: string;
  previousState: string | null;
  changedAt: string;
}

export function useIotHub(
  machineCode: string | null,
  onSignalUpdated?: (update: MachineSignalUpdate) => void,
  onStateChanged?: (update: MachineStateUpdate) => void,
) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const onSignalRef = useRef(onSignalUpdated);
  const onStateRef = useRef(onStateChanged);

  onSignalRef.current = onSignalUpdated;
  onStateRef.current = onStateChanged;

  const getToken = useCallback(() => {
    return localStorage.getItem('token') ?? '';
  }, []);

  useEffect(() => {
    if (!machineCode) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/iot', {
        accessTokenFactory: getToken,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on('MachineSignalUpdated', (payload: MachineSignalUpdate) => {
      if (payload.machineCode === machineCode) onSignalRef.current?.(payload);
    });

    connection.on('MachineStateChanged', (payload: MachineStateUpdate) => {
      if (payload.machineCode === machineCode) onStateRef.current?.(payload);
    });

    connection.start()
      .then(() => connection.invoke('SubscribeMachine', machineCode))
      .catch((err) => console.warn('IoT hub connect failed:', err));

    return () => {
      connection.stop().catch(() => { /* ignore */ });
      connectionRef.current = null;
    };
  }, [machineCode, getToken]);

  return connectionRef;
}
