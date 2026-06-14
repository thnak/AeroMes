import ReactApexChart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from '@mui/material/styles';
import { Box, Skeleton, Typography } from '@mui/material';

export interface ShiftData {
  shift: string;
  ok: number;
  ng: number;
}

interface ShiftComparisonBarProps {
  data: ShiftData[];
  height?: number;
  loading?: boolean;
}

export default function ShiftComparisonBar({
  data,
  height = 280,
  loading = false,
}: ShiftComparisonBarProps) {
  const theme = useTheme();

  if (loading) return <Skeleton variant="rectangular" height={height} sx={{ borderRadius: 2 }} />;
  if (data.length === 0) {
    return (
      <Box sx={{ height, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Typography color="text.disabled" variant="body2">No shift data</Typography>
      </Box>
    );
  }

  const series: ApexOptions['series'] = [
    { name: 'OK', data: data.map((d) => d.ok) },
    { name: 'NG', data: data.map((d) => d.ng) },
  ];

  const options: ApexOptions = {
    chart: {
      type: 'bar',
      toolbar: { show: false },
      fontFamily: theme.typography.fontFamily,
      animations: { enabled: false },
    },
    colors: [theme.palette.success.main, theme.palette.error.main],
    plotOptions: {
      bar: {
        columnWidth: '55%',
        borderRadius: 3,
        dataLabels: { position: 'top' },
      },
    },
    dataLabels: {
      enabled: true,
      style: { fontSize: '11px', colors: [theme.palette.text.primary] },
      offsetY: -18,
    },
    xaxis: {
      categories: data.map((d) => d.shift),
      labels: { style: { colors: theme.palette.text.secondary } },
      axisBorder: { show: false },
      axisTicks: { show: false },
    },
    yaxis: {
      labels: { style: { colors: theme.palette.text.secondary } },
    },
    grid: { borderColor: theme.palette.divider, strokeDashArray: 4 },
    tooltip: { theme: theme.palette.mode, shared: true, intersect: false },
    legend: { position: 'top' },
  };

  return (
    <Box>
      <ReactApexChart options={options} series={series} type="bar" height={height} />
    </Box>
  );
}
