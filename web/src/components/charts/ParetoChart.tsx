import ReactApexChart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from '@mui/material/styles';

export interface ParetoItem {
  label: string;
  value: number;
}

interface ParetoChartProps {
  data: ParetoItem[];
  unit?: string;
  height?: number;
}

export default function ParetoChart({ data, unit = 'min', height = 300 }: ParetoChartProps) {
  const theme = useTheme();
  const sorted = [...data].sort((a, b) => b.value - a.value);
  const total = sorted.reduce((s, d) => s + d.value, 0);

  let cumulative = 0;
  const cumulativePct = sorted.map((d) => {
    cumulative += d.value;
    return Math.round((cumulative / total) * 100);
  });

  const options: ApexOptions = {
    chart: { toolbar: { show: false }, fontFamily: theme.typography.fontFamily },
    plotOptions: { bar: { columnWidth: '60%', borderRadius: 4 } },
    xaxis: {
      categories: sorted.map((d) => d.label),
      labels: { style: { colors: theme.palette.text.secondary, fontSize: '12px' } },
    },
    yaxis: [
      {
        title: { text: unit, style: { color: theme.palette.text.secondary, fontSize: '12px' } },
        labels: { style: { colors: theme.palette.text.secondary } },
      },
      {
        opposite: true,
        min: 0,
        max: 100,
        title: { text: '%', style: { color: theme.palette.text.secondary, fontSize: '12px' } },
        labels: {
          formatter: (v) => `${v}%`,
          style: { colors: theme.palette.text.secondary },
        },
      },
    ],
    colors: [theme.palette.primary.main, theme.palette.error.main],
    stroke: { width: [0, 2], curve: 'smooth' },
    markers: { size: [0, 4] },
    legend: { show: false },
    grid: { borderColor: theme.palette.divider },
    tooltip: {
      theme: theme.palette.mode,
      y: [
        { formatter: (v) => `${v} ${unit}` },
        { formatter: (v) => `${v}%` },
      ],
    },
  };

  return (
    <ReactApexChart
      type="bar"
      series={[
        { name: 'Duration', type: 'column', data: sorted.map((d) => d.value) },
        { name: 'Cumulative %', type: 'line', data: cumulativePct },
      ]}
      options={options}
      height={height}
    />
  );
}
