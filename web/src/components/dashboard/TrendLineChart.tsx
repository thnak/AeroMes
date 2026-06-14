import ReactApexChart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from '@mui/material/styles';
import { Box, Skeleton } from '@mui/material';

export interface TrendPoint {
  label: string;
  value: number;
  secondary?: number;
}

interface TrendLineChartProps {
  data: TrendPoint[];
  primaryLabel: string;
  secondaryLabel?: string;
  height?: number;
  unit?: string;
  loading?: boolean;
}

export default function TrendLineChart({
  data,
  primaryLabel,
  secondaryLabel,
  height = 240,
  unit = '',
  loading = false,
}: TrendLineChartProps) {
  const theme = useTheme();

  if (loading) return <Skeleton variant="rectangular" height={height} sx={{ borderRadius: 2 }} />;

  const series: ApexOptions['series'] = [
    { name: primaryLabel, data: data.map((d) => d.value) },
    ...(secondaryLabel
      ? [{ name: secondaryLabel, data: data.map((d) => d.secondary ?? 0) }]
      : []),
  ];

  const options: ApexOptions = {
    chart: {
      type: 'line',
      toolbar: { show: false },
      fontFamily: theme.typography.fontFamily,
      animations: { enabled: false },
    },
    stroke: { curve: 'smooth', width: 2 },
    colors: [theme.palette.primary.main, theme.palette.secondary.main],
    xaxis: {
      categories: data.map((d) => d.label),
      labels: { style: { colors: theme.palette.text.secondary } },
      axisBorder: { show: false },
      axisTicks: { show: false },
    },
    yaxis: {
      labels: {
        style: { colors: theme.palette.text.secondary },
        formatter: (v) => `${v.toFixed(0)}${unit ? ' ' + unit : ''}`,
      },
    },
    grid: { borderColor: theme.palette.divider, strokeDashArray: 4 },
    tooltip: { theme: theme.palette.mode },
    legend: { show: !!secondaryLabel },
    markers: { size: 3, colors: [theme.palette.primary.main, theme.palette.secondary.main] },
  };

  return (
    <Box>
      <ReactApexChart options={options} series={series} type="line" height={height} />
    </Box>
  );
}
