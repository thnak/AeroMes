import ReactApexChart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from '@mui/material/styles';
import { alpha } from '@mui/material/styles';
import { Box, LinearProgress, Skeleton, Stack, Typography } from '@mui/material';
import { oeeZoneColor } from '../../theme/tokens';

interface OeeGaugeProps {
  oee: number;
  availability: number;
  performance: number;
  quality: number;
  machineCode: string;
  loading?: boolean;
}

function ComponentBar({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <Box>
      <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
        <Typography variant="caption" color="text.secondary">{label}</Typography>
        <Typography variant="caption" sx={{ fontWeight: 600, color }}>{value.toFixed(1)}%</Typography>
      </Stack>
      <LinearProgress
        variant="determinate"
        value={Math.min(100, value)}
        sx={{
          height: 6,
          borderRadius: 3,
          bgcolor: alpha(color, 0.12),
          '& .MuiLinearProgress-bar': { bgcolor: color, borderRadius: 3 },
        }}
      />
    </Box>
  );
}

export default function OeeGauge({
  oee,
  availability,
  performance,
  quality,
  machineCode,
  loading = false,
}: OeeGaugeProps) {
  const theme = useTheme();
  const color = oeeZoneColor(oee);

  if (loading) return <Skeleton variant="rectangular" height={280} sx={{ borderRadius: 2 }} />;

  const options: ApexOptions = {
    chart: { type: 'radialBar', sparkline: { enabled: true } },
    plotOptions: {
      radialBar: {
        startAngle: -135,
        endAngle: 135,
        hollow: { size: '60%' },
        dataLabels: {
          name: { show: true, offsetY: -10, color: theme.palette.text.secondary, fontSize: '11px' },
          value: {
            show: true,
            fontSize: '22px',
            fontWeight: 700,
            color,
            formatter: (v) => `${Number(v).toFixed(1)}%`,
          },
        },
        track: { background: alpha(color, 0.12) },
      },
    },
    fill: { type: 'solid', colors: [color] },
    labels: [machineCode],
    tooltip: { enabled: false },
  };

  return (
    <Stack spacing={1.5}>
      <ReactApexChart options={options} series={[oee]} type="radialBar" height={180} />
      <Stack spacing={1} sx={{ px: 1, pb: 1 }}>
        <ComponentBar label="Availability" value={availability} color={theme.palette.success.main} />
        <ComponentBar label="Performance" value={performance} color={theme.palette.primary.main} />
        <ComponentBar label="Quality" value={quality} color={theme.palette.info.main} />
      </Stack>
    </Stack>
  );
}
