import ReactApexChart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from '@mui/material/styles';
import { alpha } from '@mui/material/styles';
import { oeeZones } from '../../theme/tokens';

export interface OeeTrendPoint {
  date: string;    // ISO date or label
  oee: number;
  availability?: number;
  performance?: number;
  quality?: number;
}

interface OeeTrendChartProps {
  data: OeeTrendPoint[];
  showComponents?: boolean;
  height?: number;
}

export default function OeeTrendChart({ data, showComponents = false, height = 300 }: OeeTrendChartProps) {
  const theme = useTheme();

  const series = [
    { name: 'OEE', data: data.map((d) => [d.date, d.oee]) },
    ...(showComponents
      ? [
          { name: 'Availability', data: data.map((d) => [d.date, d.availability ?? null]) },
          { name: 'Performance',  data: data.map((d) => [d.date, d.performance ?? null]) },
          { name: 'Quality',      data: data.map((d) => [d.date, d.quality ?? null]) },
        ]
      : []),
  ];

  const options: ApexOptions = {
    chart: {
      type: 'area',
      toolbar: { show: false },
      fontFamily: theme.typography.fontFamily,
      animations: { enabled: false },
    },
    stroke: { curve: 'smooth', width: 2 },
    fill: {
      type: 'gradient',
      gradient: {
        shadeIntensity: 1,
        opacityFrom: 0.25,
        opacityTo: 0,
        stops: [0, 100],
      },
    },
    colors: [
      oeeZones.GOOD.color,
      theme.palette.info.main,
      theme.palette.warning.main,
      theme.palette.success.main,
    ],
    xaxis: {
      type: 'category',
      labels: { style: { colors: theme.palette.text.secondary, fontSize: '12px' } },
    },
    yaxis: {
      min: 0,
      max: 100,
      tickAmount: 5,
      labels: {
        formatter: (v) => `${v}%`,
        style: { colors: theme.palette.text.secondary },
      },
    },
    grid: { borderColor: theme.palette.divider },
    annotations: {
      yaxis: [
        {
          y: 85,
          borderColor: alpha(oeeZones.WORLD_CLASS.color, 0.4),
          label: {
            text: 'World class 85%',
            style: { color: oeeZones.WORLD_CLASS.color, fontSize: '11px', background: 'transparent' },
          },
        },
      ],
    },
    tooltip: {
      theme: theme.palette.mode,
      y: { formatter: (v) => `${v?.toFixed(1)}%` },
    },
    legend: {
      show: showComponents,
      labels: { colors: theme.palette.text.secondary },
    },
  };

  return (
    <ReactApexChart
      type="area"
      series={series as never}
      options={options}
      height={height}
    />
  );
}
