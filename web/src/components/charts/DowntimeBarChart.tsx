import ReactApexChart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from '@mui/material/styles';

export interface DowntimeSeries {
  name: string;   // machine or category name
  data: number[]; // minutes per category/date
}

interface DowntimeBarChartProps {
  categories: string[];
  series: DowntimeSeries[];
  horizontal?: boolean;
  height?: number;
}

export default function DowntimeBarChart({
  categories,
  series,
  horizontal = false,
  height = 300,
}: DowntimeBarChartProps) {
  const theme = useTheme();

  const options: ApexOptions = {
    chart: {
      type: 'bar',
      stacked: true,
      toolbar: { show: false },
      fontFamily: theme.typography.fontFamily,
    },
    plotOptions: {
      bar: {
        horizontal,
        columnWidth: '55%',
        barHeight: '70%',
        borderRadius: horizontal ? 3 : 4,
        borderRadiusApplication: 'end',
      },
    },
    xaxis: {
      categories,
      labels: { style: { colors: theme.palette.text.secondary, fontSize: '12px' } },
    },
    yaxis: {
      labels: {
        formatter: (v) => `${v}m`,
        style: { colors: theme.palette.text.secondary },
      },
    },
    colors: [
      theme.palette.error.main,
      theme.palette.warning.main,
      theme.palette.info.main,
      theme.palette.text.disabled,
    ],
    grid: { borderColor: theme.palette.divider },
    legend: { labels: { colors: theme.palette.text.secondary } },
    tooltip: {
      theme: theme.palette.mode,
      y: { formatter: (v) => `${v} min` },
    },
    dataLabels: { enabled: false },
  };

  return (
    <ReactApexChart
      type="bar"
      series={series}
      options={options}
      height={height}
    />
  );
}
