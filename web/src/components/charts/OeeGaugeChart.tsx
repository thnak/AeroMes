import ReactApexChart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from '@mui/material/styles';
import { oeeZoneColor } from '../../theme/tokens';

interface OeeGaugeChartProps {
  value: number;       // 0–100
  label?: string;
  height?: number;
}

export default function OeeGaugeChart({ value, label = 'OEE', height = 220 }: OeeGaugeChartProps) {
  const theme = useTheme();
  const color = oeeZoneColor(value);

  const options: ApexOptions = {
    chart: { type: 'radialBar', sparkline: { enabled: true } },
    plotOptions: {
      radialBar: {
        startAngle: -135,
        endAngle: 135,
        track: { background: theme.palette.divider, strokeWidth: '100%' },
        dataLabels: {
          name: {
            offsetY: 24,
            fontSize: '13px',
            color: theme.palette.text.secondary,
            fontFamily: theme.typography.fontFamily,
          },
          value: {
            offsetY: -8,
            fontSize: '28px',
            fontWeight: 700,
            color,
            fontFamily: theme.typography.fontFamily,
            formatter: (val) => `${val}%`,
          },
        },
        hollow: { size: '58%' },
      },
    },
    fill: { type: 'solid', colors: [color] },
    stroke: { lineCap: 'round' },
    labels: [label],
  };

  return (
    <ReactApexChart
      type="radialBar"
      series={[Math.min(100, Math.max(0, value))]}
      options={options}
      height={height}
    />
  );
}
