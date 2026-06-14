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
import androidx.compose.material3.OutlinedButton
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
import com.iotviet.aeromes.domain.model.WorkOrder
import com.iotviet.aeromes.ui.jobs.EmptyMessage
import com.iotviet.aeromes.ui.jobs.ErrorMessage

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MaterialIssueScreen(
    onBack: () -> Unit,
    vm: MaterialIssueViewModel = hiltViewModel()
) {
    val state by vm.uiState.collectAsStateWithLifecycle()
    val snackbar = remember { SnackbarHostState() }

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
                title = { Text("Material Issue") },
                navigationIcon = {
                    if (state.step == MaterialIssueStep.SELECT_WO) {
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
            StepIndicator(
                steps = listOf("Select WO", "Scan Lot", "Confirm"),
                currentStep = state.step.ordinal,
                modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp)
            )
            HorizontalDivider()

            when (state.step) {
                MaterialIssueStep.SELECT_WO -> SelectWoStep(
                    workOrders = state.workOrders,
                    isLoading = state.isLoadingWo,
                    error = state.error,
                    onSelect = vm::selectWorkOrder
                )
                MaterialIssueStep.SCAN_LOT -> ScanLotStep(
                    lotNumber = state.lotNumber,
                    lotItem = state.lotItem,
                    quantity = state.quantity,
                    notes = state.notes,
                    isLoadingLot = state.isLoadingLot,
                    error = state.error,
                    selectedWo = state.selectedWo!!,
                    onLotNumberChange = vm::onLotNumberChange,
                    onQuantityChange = vm::onQuantityChange,
                    onNotesChange = vm::onNotesChange,
                    onLookupLot = vm::lookupLot,
                    onNext = vm::proceedToConfirm
                )
                MaterialIssueStep.CONFIRM -> ConfirmStep(
                    state = state,
                    onSubmit = vm::submit
                )
            }
        }
    }
}

@Composable
private fun StepIndicator(steps: List<String>, currentStep: Int, modifier: Modifier = Modifier) {
    Row(modifier = modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(4.dp)) {
        steps.forEachIndexed { index, label ->
            Column(modifier = Modifier.weight(1f), horizontalAlignment = Alignment.CenterHorizontally) {
                val active = index == currentStep
                val done = index < currentStep
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
}

@Composable
private fun SelectWoStep(
    workOrders: List<WorkOrder>,
    isLoading: Boolean,
    error: String?,
    onSelect: (WorkOrder) -> Unit
) {
    when {
        isLoading -> Column(Modifier.fillMaxSize(), horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center) { CircularProgressIndicator() }
        error != null -> ErrorMessage(error)
        workOrders.isEmpty() -> EmptyMessage("No released work orders available.")
        else -> LazyColumn(contentPadding = PaddingValues(16.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
            item { Text("Select Work Order", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.SemiBold) }
            items(workOrders, key = { it.id }) { wo ->
                Card(onClick = { onSelect(wo) }, modifier = Modifier.fillMaxWidth()) {
                    Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(4.dp)) {
                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                            Text(wo.code, style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.SemiBold)
                            Text(wo.status, style = MaterialTheme.typography.labelSmall,
                                color = MaterialTheme.colorScheme.primary)
                        }
                        Text(wo.workCenterName, style = MaterialTheme.typography.bodySmall)
                        Text("Qty: ${wo.actualOK}/${wo.targetQty}", style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant)
                    }
                }
            }
        }
    }
}

@Composable
private fun ScanLotStep(
    lotNumber: String,
    lotItem: com.iotviet.aeromes.domain.model.InventoryItem?,
    quantity: String,
    notes: String,
    isLoadingLot: Boolean,
    error: String?,
    selectedWo: WorkOrder,
    onLotNumberChange: (String) -> Unit,
    onQuantityChange: (String) -> Unit,
    onNotesChange: (String) -> Unit,
    onLookupLot: () -> Unit,
    onNext: () -> Unit
) {
    val keyboard = LocalSoftwareKeyboardController.current
    Column(
        modifier = Modifier.fillMaxSize().padding(16.dp).verticalScroll(rememberScrollState()),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        Card(
            modifier = Modifier.fillMaxWidth(),
            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.secondaryContainer)
        ) {
            Text(
                "WO: ${selectedWo.code}",
                modifier = Modifier.padding(12.dp),
                style = MaterialTheme.typography.bodyMedium,
                fontWeight = FontWeight.SemiBold
            )
        }

        OutlinedTextField(
            value = lotNumber,
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

        if (isLoadingLot) CircularProgressIndicator()

        error?.let { Text(it, color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall) }

        lotItem?.let { item ->
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(4.dp)) {
                    Text("${item.productCode} — ${item.productName}", style = MaterialTheme.typography.bodyMedium,
                        fontWeight = FontWeight.SemiBold)
                    Text("Available: ${item.quantity} ${item.unit ?: ""}", style = MaterialTheme.typography.bodySmall)
                    val loc = listOfNotNull(item.warehouseCode, item.locationCode).joinToString(" / ")
                    if (loc.isNotBlank()) Text("Location: $loc", style = MaterialTheme.typography.bodySmall)
                }
            }

            OutlinedTextField(
                value = quantity,
                onValueChange = onQuantityChange,
                label = { Text("Quantity to Issue") },
                singleLine = true,
                modifier = Modifier.fillMaxWidth(),
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal)
            )

            OutlinedTextField(
                value = notes,
                onValueChange = onNotesChange,
                label = { Text("Notes (optional)") },
                modifier = Modifier.fillMaxWidth(),
                minLines = 2
            )

            Button(onClick = onNext, modifier = Modifier.fillMaxWidth()) {
                Text("Next: Review & Confirm")
            }
        }
        Spacer(Modifier.height(16.dp))
    }
}

@Composable
private fun ConfirmStep(state: MaterialIssueUiState, onSubmit: () -> Unit) {
    val wo = state.selectedWo!!
    val item = state.lotItem!!
    Column(
        modifier = Modifier.fillMaxSize().padding(16.dp).verticalScroll(rememberScrollState()),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        Text("Review & Confirm", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
        Card(modifier = Modifier.fillMaxWidth()) {
            Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                ConfirmRow("Work Order", wo.code)
                ConfirmRow("Product", "${item.productCode} — ${item.productName}")
                ConfirmRow("Lot Number", item.lotNumber ?: state.lotNumber)
                ConfirmRow("Quantity", "${state.quantity} ${item.unit ?: ""}")
                if (state.notes.isNotBlank()) ConfirmRow("Notes", state.notes)
            }
        }

        state.error?.let { Text(it, color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall) }

        Button(onClick = onSubmit, enabled = !state.isSubmitting, modifier = Modifier.fillMaxWidth()) {
            if (state.isSubmitting) CircularProgressIndicator(modifier = Modifier.height(16.dp), strokeWidth = 2.dp)
            else Text("Confirm Issue Material")
        }
        Spacer(Modifier.height(16.dp))
    }
}

@Composable
private fun ConfirmRow(label: String, value: String) {
    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
        Text(label, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
        Text(value, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold)
    }
}
