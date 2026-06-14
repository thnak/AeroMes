import { useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

export interface MachineStatusChanged {
  machineCode: string;
  status: string;
  timestamp: string;
}

export interface WorkOrderProgressUpdated {
  workOrderId: number;
  woCode: string;
  actualOk: number;
  actualNg: number;
  completionPct: number;
}

export interface WorkOrderStarted {
  workOrderId: number;
  woCode: string;
  workCenterId: number;
}

export interface WorkOrderCompleted {
  workOrderId: number;
  woCode: string;
}

export interface ShopFloorCallbacks {
  onMachineStatusChanged?: (payload: MachineStatusChanged) => void;
  onWorkOrderProgressUpdated?: (payload: WorkOrderProgressUpdated) => void;
  onWorkOrderStarted?: (payload: WorkOrderStarted) => void;
  onWorkOrderCompleted?: (payload: WorkOrderCompleted) => void;
}

export function useShopFloor(workCenterId?: number, callbacks?: ShopFloorCallbacks) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const cbRef = useRef(callbacks);
  cbRef.current = callbacks;

  const getToken = useCallback(() => localStorage.getItem('token') ?? '', []);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/shop-floor', {
        accessTokenFactory: getToken,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on('MachineStatusChanged', (p: MachineStatusChanged) => cbRef.current?.onMachineStatusChanged?.(p));
    connection.on('WorkOrderProgressUpdated', (p: WorkOrderProgressUpdated) => cbRef.current?.onWorkOrderProgressUpdated?.(p));
    connection.on('WorkOrderStarted', (p: WorkOrderStarted) => cbRef.current?.onWorkOrderStarted?.(p));
    connection.on('WorkOrderCompleted', (p: WorkOrderCompleted) => cbRef.current?.onWorkOrderCompleted?.(p));

    connection.start()
      .then(async () => {
        await connection.invoke('JoinFactory');
        if (workCenterId) await connection.invoke('JoinWorkCenter', workCenterId);
      })
      .catch((err) => console.warn('[ShopFloor] hub connect failed:', err));

    return () => {
      connection.stop().catch(() => { /* ignore */ });
      connectionRef.current = null;
    };
  }, [workCenterId, getToken]);

  return connectionRef;
}
