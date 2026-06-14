import ReactApexChart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from '@mui/material/styles';
import { Box, Skeleton, Typography } from '@mui/material';

interface DonutSegment {
  label: string;
  value: number;
  color: string;
}

interface StatusDonutProps {
  segments: DonutSegment[];
  total?: number;
  centerLabel?: string;
  height?: number;
  loading?: boolean;
}

export default function StatusDonut({
  segments,
  total,
  centerLabel,
  height = 240,
  loading = false,
}: StatusDonutProps) {
  const theme = useTheme();

  if (loading) return <Skeleton variant="circular" width={height} height={height} sx={{ mx: 'auto' }} />;

  const computed = total ?? segments.reduce((s, d) => s + d.value, 0);

  const options: ApexOptions = {
    chart: {
      type: 'donut',
      toolbar: { show: false },
      fontFamily: theme.typography.fontFamily,
      animations: { enabled: false },
    },
    colors: segments.map((s) => s.color),
    labels: segments.map((s) => s.label),
    legend: { position: 'bottom', fontSize: '12px' },
    plotOptions: {
      pie: {
        donut: {
          size: '65%',
          labels: {
            show: true,
            total: {
              show: true,
              label: centerLabel ?? 'Total',
              fontSize: '12px',
              color: theme.palette.text.secondary,
              formatter: () => String(computed),
            },
          },
        },
      },
    },
    dataLabels: { enabled: false },
    tooltip: {
      theme: theme.palette.mode,
      y: { formatter: (v) => `${v} (${computed > 0 ? ((v / computed) * 100).toFixed(1) : 0}%)` },
    },
  };

  if (segments.every((s) => s.value === 0)) {
    return (
      <Box sx={{ height, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Typography color="text.disabled" variant="body2">No data</Typography>
      </Box>
    );
  }

  return (
    <Box>
      <ReactApexChart
        options={options}
        series={segments.map((s) => s.value)}
        type="donut"
        height={height}
      />
    </Box>
  );
}
