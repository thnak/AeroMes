import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  Grid,
  MenuItem,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { PageHeader, PageRoot, SolarIcon } from '../../components';
import { apiClient } from '../../lib/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

interface LotEdge {
  parentLotNumber: string;
  childLotNumber: string;
  lineageType: string;
  quantityConsumed?: number;
  uom?: string;
  workOrderId?: number;
}

interface TraceResult {
  rootLot: string;
  nodes: unknown[];
  edges: LotEdge[];
  depth: number;
}

interface LotEventDto {
  eventId: number;
  eventType: string;
  lotNumber: string;
  productCode: string;
  operatorCode?: string;
  eventTimestamp: string;
  quantity?: number;
}

// ─── API ──────────────────────────────────────────────────────────────────────

const fetchTrace = (direction: string, lotNumber: string, depth: number): Promise<TraceResult> =>
  apiClient.get(`/api/v1/traceability/${direction}?lotNumber=${encodeURIComponent(lotNumber)}&maxDepth=${depth}`)
    .then((r) => r.data);

const fetchLotEvents = (lotNumber: string): Promise<LotEventDto[]> =>
  apiClient.get(`/api/v1/traceability/lot-events/${encodeURIComponent(lotNumber)}`)
    .then((r) => r.data);

// ─── Edge list ────────────────────────────────────────────────────────────────

function EdgeList({ edges, direction }: { edges: LotEdge[]; direction: string }) {
  if (edges.length === 0) {
    return (
      <Typography color="text.disabled" variant="body2" sx={{ py: 3, textAlign: 'center' }}>
        No {direction === 'forward' ? 'downstream' : 'upstream'} lots found.
      </Typography>
    );
  }
  return (
    <Stack spacing={1}>
      {edges.map((e, i) => (
        <Box
          key={i}
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 2,
            p: 1.5,
            borderRadius: 2,
            border: '1px solid',
            borderColor: 'divider',
          }}
        >
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
              <Typography variant="body2" sx={{ fontWeight: 600 }} noWrap>
                {direction === 'forward' ? e.childLotNumber : e.parentLotNumber}
              </Typography>
              <Chip
                label={e.lineageType}
                size="small"
                sx={{
                  bgcolor: (t) => alpha(t.palette.primary.main, 0.08),
                  color: 'primary.main',
                  fontSize: 10,
                  height: 20,
                }}
              />
            </Stack>
            {e.quantityConsumed != null && (
              <Typography variant="caption" color="text.secondary">
                {e.quantityConsumed} {e.uom ?? ''}{e.workOrderId ? ` · WO #${e.workOrderId}` : ''}
              </Typography>
            )}
          </Box>
          <SolarIcon name="routing" size={16} />
        </Box>
      ))}
    </Stack>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function GenealogyPage() {
  const [lotNumber, setLotNumber] = useState('');
  const [direction, setDirection] = useState<'backward' | 'forward' | 'bidirectional'>('backward');
  const [depth, setDepth] = useState(10);
  const [traceLot, setTraceLot] = useState('');

  const { data: traceResult, isLoading, error } = useQuery({
    queryKey: ['trace', direction, traceLot, depth],
    queryFn: () => fetchTrace(direction, traceLot, depth),
    enabled: traceLot.length > 0,
  });

  const { data: events = [] } = useQuery({
    queryKey: ['lot-events', traceLot],
    queryFn: () => fetchLotEvents(traceLot),
    enabled: traceLot.length > 0,
  });

  const handleTrace = () => {
    if (lotNumber.trim()) setTraceLot(lotNumber.trim());
  };

  return (
    <PageRoot>
      <PageHeader
        title="Genealogy Explorer"
        subtitle="Trace lot lineage forward, backward, or bidirectional"
      />

      {/* ── Search Bar ───────────────────────────────────────────────────── */}
      <Card variant="outlined" sx={{ borderRadius: 3, mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} sx={{ alignItems: 'flex-end' }}>
            <Grid size={{ xs: 12, sm: 5 }}>
              <TextField
                fullWidth
                label="Lot / Serial Number"
                value={lotNumber}
                onChange={(e) => setLotNumber(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleTrace()}
                slotProps={{
                  input: {
                    startAdornment: <SolarIcon name="search" size={18} />,
                  },
                }}
                placeholder="e.g. RM-2024-0055"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <ToggleButtonGroup
                value={direction}
                exclusive
                onChange={(_, v) => v && setDirection(v)}
                size="small"
              >
                <Tooltip title="Trace origins (upstream)">
                  <ToggleButton value="backward">Backward</ToggleButton>
                </Tooltip>
                <Tooltip title="Trace derivatives (downstream)">
                  <ToggleButton value="forward">Forward</ToggleButton>
                </Tooltip>
                <Tooltip title="Full blast radius">
                  <ToggleButton value="bidirectional">Both</ToggleButton>
                </Tooltip>
              </ToggleButtonGroup>
            </Grid>
            <Grid size={{ xs: 12, sm: 1 }}>
              <TextField
                select
                label="Depth"
                value={depth}
                onChange={(e) => setDepth(Number(e.target.value))}
                size="small"
                fullWidth
              >
                {[5, 10, 20].map((d) => (
                  <MenuItem key={d} value={d}>{d}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, sm: 2 }}>
              <Button
                fullWidth
                variant="contained"
                onClick={handleTrace}
                disabled={!lotNumber.trim() || isLoading}
                startIcon={<SolarIcon name="search" size={16} />}
              >
                Trace
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* ── Results ──────────────────────────────────────────────────────── */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to trace lot. Please check the lot number and try again.
        </Alert>
      )}

      {traceLot && !isLoading && (
        <Grid container spacing={2}>
          {/* Lineage edges */}
          <Grid size={{ xs: 12, md: 7 }}>
            <Card variant="outlined" sx={{ borderRadius: 3 }}>
              <CardContent>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
                  Lineage — {traceLot}
                </Typography>
                <Chip
                  label={`${traceResult?.edges?.length ?? 0} edge(s) found · Depth ${depth}`}
                  size="small"
                  sx={{ mb: 2 }}
                />
                <EdgeList edges={traceResult?.edges ?? []} direction={direction} />
              </CardContent>
            </Card>
          </Grid>

          {/* Event timeline */}
          <Grid size={{ xs: 12, md: 5 }}>
            <Card variant="outlined" sx={{ borderRadius: 3 }}>
              <CardContent>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
                  Event Timeline
                </Typography>
                {events.length === 0 ? (
                  <Typography color="text.disabled" variant="body2">
                    No events recorded for this lot.
                  </Typography>
                ) : (
                  <Stack divider={<Divider sx={{ my: 0.5 }} />} spacing={0}>
                    {events.map((evt) => (
                      <Box key={evt.eventId} sx={{ py: 1 }}>
                        <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start' }}>
                          <Box>
                            <Chip
                              label={evt.eventType}
                              size="small"
                              sx={{
                                bgcolor: (t) => alpha(
                                  evt.eventType === 'Received' ? t.palette.success.main
                                  : evt.eventType === 'Consumed' ? t.palette.warning.main
                                  : evt.eventType === 'Produced' ? t.palette.primary.main
                                  : t.palette.info.main,
                                  0.1
                                ),
                                color: evt.eventType === 'Received' ? 'success.main'
                                  : evt.eventType === 'Consumed' ? 'warning.main'
                                  : evt.eventType === 'Produced' ? 'primary.main'
                                  : 'info.main',
                                fontWeight: 600,
                                mb: 0.5,
                              }}
                            />
                            <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                              {evt.productCode}
                              {evt.quantity != null ? ` · Qty ${evt.quantity}` : ''}
                              {evt.operatorCode ? ` · ${evt.operatorCode}` : ''}
                            </Typography>
                          </Box>
                          <Typography variant="caption" color="text.disabled" sx={{ ml: 1, whiteSpace: 'nowrap' }}>
                            {new Date(evt.eventTimestamp).toLocaleDateString()}
                          </Typography>
                        </Stack>
                      </Box>
                    ))}
                  </Stack>
                )}
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {!traceLot && (
        <Box
          sx={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            minHeight: 280,
            gap: 2,
            borderRadius: 3,
            border: '1.5px dashed',
            borderColor: 'divider',
          }}
        >
          <Box
            sx={{
              width: 56,
              height: 56,
              borderRadius: 3,
              bgcolor: (t) => alpha(t.palette.primary.main, 0.08),
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <SolarIcon name="routing" size={28} />
          </Box>
          <Typography color="text.secondary" variant="subtitle1">
            Enter a lot number to trace its genealogy
          </Typography>
        </Box>
      )}
    </PageRoot>
  );
}
