import ReactApexChart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from '@mui/material/styles';
import { Box, Skeleton, Typography } from '@mui/material';

export interface DefectParetoItem {
  code: string;
  name: string;
  count: number;
  cumulativePct: number;
}

interface DashboardParetoChartProps {
  data: DefectParetoItem[];
  height?: number;
  loading?: boolean;
}

export default function DashboardParetoChart({
  data,
  height = 300,
  loading = false,
}: DashboardParetoChartProps) {
  const theme = useTheme();

  if (loading) return <Skeleton variant="rectangular" height={height} sx={{ borderRadius: 2 }} />;
  if (data.length === 0) {
    return (
      <Box sx={{ height, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Typography color="text.disabled" variant="body2">No defect data</Typography>
      </Box>
    );
  }

  const series: ApexOptions['series'] = [
    { name: 'Count', type: 'column', data: data.map((d) => d.count) },
    { name: 'Cumulative %', type: 'line', data: data.map((d) => d.cumulativePct) },
  ];

  const options: ApexOptions = {
    chart: {
      type: 'line',
      toolbar: { show: false },
      fontFamily: theme.typography.fontFamily,
      animations: { enabled: false },
    },
    stroke: { width: [0, 2], curve: 'smooth' },
    colors: [theme.palette.error.main, theme.palette.warning.main],
    plotOptions: { bar: { columnWidth: '60%', borderRadius: 3 } },
    xaxis: {
      categories: data.map((d) => d.code),
      labels: { style: { colors: theme.palette.text.secondary }, rotate: -30 },
      axisBorder: { show: false },
    },
    yaxis: [
      {
        labels: { style: { colors: theme.palette.text.secondary } },
        title: { text: 'Count', style: { color: theme.palette.text.secondary } },
      },
      {
        opposite: true,
        max: 100,
        labels: {
          style: { colors: theme.palette.text.secondary },
          formatter: (v) => `${v.toFixed(0)}%`,
        },
        title: { text: 'Cumulative %', style: { color: theme.palette.text.secondary } },
      },
    ],
    grid: { borderColor: theme.palette.divider, strokeDashArray: 4 },
    tooltip: {
      theme: theme.palette.mode,
      shared: true,
      custom: ({ dataPointIndex }) => {
        const d = data[dataPointIndex];
        return `<div style="padding:8px 12px">
          <b>${d.code}</b>: ${d.name}<br/>
          Count: ${d.count} · Cumulative: ${d.cumulativePct}%
        </div>`;
      },
    },
    legend: { position: 'top' },
  };

  return (
    <Box>
      <ReactApexChart options={options} series={series} type="line" height={height} />
    </Box>
  );
}
