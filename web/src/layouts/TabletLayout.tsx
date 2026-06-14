import { Outlet } from 'react-router-dom';
import { TabletSessionProvider } from '../contexts/TabletSessionContext';

export default function TabletLayout() {
  return (
    <TabletSessionProvider>
      <Outlet />
    </TabletSessionProvider>
  );
}
