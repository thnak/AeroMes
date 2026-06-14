package com.iotviet.aeromes.ui.material

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalSoftwareKeyboardController
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.iotviet.aeromes.domain.model.LotTrace
import com.iotviet.aeromes.domain.model.StorageLocation
import com.iotviet.aeromes.ui.jobs.EmptyMessage
import com.iotviet.aeromes.ui.jobs.ErrorMessage

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun StockTransferScreen(
    initialLotNumber: String? = null,
    onBack: () -> Unit,
    vm: StockTransferViewModel = hiltViewModel()
) {
    val state by vm.uiState.collectAsStateWithLifecycle()
    val snackbar = remember { SnackbarHostState() }

    LaunchedEffect(initialLotNumber) {
        if (!initialLotNumber.isNullOrBlank()) {
            vm.onLotNumberChange(initialLotNumber)
            vm.lookupLot()
        }
    }

    LaunchedEffect(state.successMessage) {
        state.successMessage?.let {
            snackbar.showSnackbar(it)
            vm.clearSuccess()
        }
    }

    Scaffold(
        snackbarHost = { SnackbarHost(snackbar) },
        topBar = {
            TopAppBar(
                title = { Text("Stock Transfer") },
                navigationIcon = {
                    if (state.step == StockTransferStep.SCAN_LOT) {
                        IconButton(onClick = onBack) { Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = null) }
                    } else {
                        IconButton(onClick = vm::goBack) { Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = null) }
                    }
                }
            )
        }
    ) { padding ->
        Column(modifier = Modifier.fillMaxSize().padding(padding)) {
            // Step indicator
            Row(
                modifier = Modifier.fillMaxWidth().padding(horizontal = 16.dp, vertical = 8.dp),
                horizontalArrangement = Arrangement.spacedBy(4.dp)
            ) {
                listOf("Scan Lot", "Pick Location", "Confirm").forEachIndexed { index, label ->
                    Column(modifier = Modifier.weight(1f), horizontalAlignment = Alignment.CenterHorizontally) {
                        val active = index == state.step.ordinal
                        val done = index < state.step.ordinal
                        LinearProgressIndicator(
                            progress = { if (done) 1f else if (active) 0.5f else 0f },
                            modifier = Modifier.fillMaxWidth().height(4.dp)
                        )
                        Text(
                            label,
                            style = MaterialTheme.typography.labelSmall,
                            color = if (active || done) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
            }
            HorizontalDivider()

            when (state.step) {
                StockTransferStep.SCAN_LOT -> ScanLotStep(state, vm::onLotNumberChange, vm::lookupLot, vm::onQuantityChange, vm::proceedToPickLocation)
                StockTransferStep.PICK_LOCATION -> PickLocationStep(state.locations, state.isLoadingLocations, state.error, vm::selectLocation)
                StockTransferStep.CONFIRM -> TransferConfirmStep(state, vm::submit)
            }
        }
    }
}

@Composable
private fun ScanLotStep(
    state: StockTransferUiState,
    onLotNumberChange: (String) -> Unit,
    onLookupLot: () -> Unit,
    onQuantityChange: (String) -> Unit,
    onNext: () -> Unit
) {
    val keyboard = LocalSoftwareKeyboardController.current
    Column(
        modifier = Modifier.fillMaxSize().padding(16.dp).verticalScroll(rememberScrollState()),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        OutlinedTextField(
            value = state.lotNumber,
            onValueChange = onLotNumberChange,
            label = { Text("Lot Number (scan or type)") },
            singleLine = true,
            modifier = Modifier.fillMaxWidth(),
            keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search),
            keyboardActions = KeyboardActions(onSearch = { onLookupLot(); keyboard?.hide() }),
            trailingIcon = {
                IconButton(onClick = { onLookupLot(); keyboard?.hide() }) {
                    Icon(Icons.Default.Search, contentDescription = "Lookup")
                }
            }
        )

        if (state.isLoadingLot) CircularProgressIndicator()

        state.error?.let { Text(it, color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall) }

        state.lotTrace?.let { trace ->
            LotSummaryCard(trace)

            OutlinedTextField(
                value = state.quantity,
                onValueChange = onQuantityChange,
                label = { Text("Quantity to Transfer") },
                singleLine = true,
                modifier = Modifier.fillMaxWidth(),
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal)
            )

            Button(onClick = onNext, modifier = Modifier.fillMaxWidth()) {
                Text("Next: Pick Destination")
            }
        }
        Spacer(Modifier.height(16.dp))
    }
}

@Composable
private fun LotSummaryCard(trace: LotTrace) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(4.dp)) {
            Text("Lot: ${trace.lotNumber}", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold)
            Text("${trace.productCode} — ${trace.productName}", style = MaterialTheme.typography.bodyMedium)
            Text("Qty: ${trace.quantity} ${trace.unit ?: ""}", style = MaterialTheme.typography.bodySmall)
            val loc = listOfNotNull(trace.warehouseCode, trace.locationCode).joinToString(" / ")
            if (loc.isNotBlank()) Text("Current Location: $loc", style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant)
        }
    }
}

@Composable
private fun PickLocationStep(
    locations: List<StorageLocation>,
    isLoading: Boolean,
    error: String?,
    onSelect: (StorageLocation) -> Unit
) {
    when {
        isLoading -> Column(Modifier.fillMaxSize(), horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center) { CircularProgressIndicator() }
        error != null -> ErrorMessage(error)
        locations.isEmpty() -> EmptyMessage("No storage locations available.")
        else -> LazyColumn(contentPadding = PaddingValues(16.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
            item { Text("Select Destination", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.SemiBold) }
            items(locations, key = { it.id }) { loc ->
                Card(onClick = { onSelect(loc) }, modifier = Modifier.fillMaxWidth()) {
                    Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(4.dp)) {
                        Text(loc.code, style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.SemiBold)
                        loc.name?.let { Text(it, style = MaterialTheme.typography.bodySmall) }
                        loc.warehouseCode?.let {
                            Text("Warehouse: $it", style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant)
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun TransferConfirmStep(state: StockTransferUiState, onSubmit: () -> Unit) {
    val trace = state.lotTrace!!
    val dest = state.selectedLocation!!
    Column(
        modifier = Modifier.fillMaxSize().padding(16.dp).verticalScroll(rememberScrollState()),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        Text("Confirm Transfer", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
        Card(modifier = Modifier.fillMaxWidth()) {
            Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                TransferRow("Lot Number", trace.lotNumber)
                TransferRow("Product", "${trace.productCode} — ${trace.productName}")
                TransferRow("Quantity", "${state.quantity} ${trace.unit ?: ""}")
                HorizontalDivider()
                val srcLoc = listOfNotNull(trace.warehouseCode, trace.locationCode).joinToString(" / ")
                TransferRow("From", srcLoc.ifBlank { "-" })
                TransferRow("To", "${dest.code}${dest.name?.let { " ($it)" } ?: ""}")
            }
        }

        state.error?.let { Text(it, color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall) }

        Button(onClick = onSubmit, enabled = !state.isSubmitting, modifier = Modifier.fillMaxWidth()) {
            if (state.isSubmitting) CircularProgressIndicator(modifier = Modifier.height(16.dp), strokeWidth = 2.dp)
            else Text("Confirm Transfer")
        }
        Spacer(Modifier.height(16.dp))
    }
}

@Composable
private fun TransferRow(label: String, value: String) {
    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
        Text(label, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
        Text(value, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold)
    }
}
