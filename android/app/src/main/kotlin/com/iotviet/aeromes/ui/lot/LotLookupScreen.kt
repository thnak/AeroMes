package com.iotviet.aeromes.ui.lot

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Lock
import androidx.compose.material.icons.filled.LockOpen
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalSoftwareKeyboardController
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.iotviet.aeromes.domain.model.HoldStatus
import com.iotviet.aeromes.domain.model.LotTrace
import com.iotviet.aeromes.ui.jobs.EmptyMessage
import com.iotviet.aeromes.ui.jobs.ErrorMessage

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LotLookupScreen(
    initialLotNumber: String? = null,
    onNavigateToHold: (String) -> Unit,
    onNavigateToTransfer: (String) -> Unit,
    onBack: () -> Unit,
    vm: LotLookupViewModel = hiltViewModel()
) {
    val state by vm.uiState.collectAsStateWithLifecycle()
    val keyboard = LocalSoftwareKeyboardController.current

    LaunchedEffect(initialLotNumber) {
        if (!initialLotNumber.isNullOrBlank()) vm.prefill(initialLotNumber)
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Lot Lookup") },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = null)
                    }
                }
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(horizontal = 16.dp)
        ) {
            Spacer(Modifier.height(8.dp))
            OutlinedTextField(
                value = state.lotNumber,
                onValueChange = vm::onLotNumberChange,
                label = { Text("Lot Number") },
                singleLine = true,
                modifier = Modifier.fillMaxWidth(),
                keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search),
                keyboardActions = KeyboardActions(onSearch = {
                    vm.lookup()
                    keyboard?.hide()
                }),
                trailingIcon = {
                    IconButton(onClick = { vm.lookup(); keyboard?.hide() }) {
                        Icon(Icons.Default.Search, contentDescription = "Search")
                    }
                }
            )
            Spacer(Modifier.height(16.dp))

            when {
                state.isLoading -> Column(
                    modifier = Modifier.fillMaxSize(),
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.Center
                ) { CircularProgressIndicator() }

                state.error != null -> ErrorMessage(state.error!!)

                state.lotTrace == null -> EmptyMessage("Enter a lot number to look up.")

                else -> Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .verticalScroll(rememberScrollState()),
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    LotDetailCard(state.lotTrace!!)
                    state.holdStatus?.let { HoldStatusCard(it) }
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        OutlinedButton(
                            onClick = { onNavigateToTransfer(state.lotTrace!!.lotNumber) },
                            modifier = Modifier.weight(1f)
                        ) { Text("Transfer") }
                        Button(
                            onClick = { onNavigateToHold(state.lotTrace!!.lotNumber) },
                            modifier = Modifier.weight(1f)
                        ) {
                            val isHeld = state.holdStatus?.isHeld == true
                            Text(if (isHeld) "Release Hold" else "Place Hold")
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun LotDetailCard(lot: LotTrace) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(6.dp)) {
            Text("Lot Detail", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
            LabeledRow("Lot Number", lot.lotNumber)
            LabeledRow("Product Code", lot.productCode)
            LabeledRow("Product Name", lot.productName)
            LabeledRow("Quantity", "${lot.quantity} ${lot.unit ?: ""}")
            val location = listOfNotNull(lot.warehouseCode, lot.locationCode).joinToString(" / ")
            if (location.isNotBlank()) LabeledRow("Location", location)
            lot.expiryDate?.let { LabeledRow("Expiry", it) }
        }
    }
}

@Composable
private fun HoldStatusCard(hold: HoldStatus) {
    val isHeld = hold.isHeld
    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(
            containerColor = if (isHeld) MaterialTheme.colorScheme.errorContainer
            else MaterialTheme.colorScheme.secondaryContainer
        )
    ) {
        Row(
            modifier = Modifier.padding(16.dp).fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            Icon(
                imageVector = if (isHeld) Icons.Default.Lock else Icons.Default.LockOpen,
                contentDescription = null,
                tint = if (isHeld) MaterialTheme.colorScheme.onErrorContainer
                else MaterialTheme.colorScheme.onSecondaryContainer
            )
            Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                Text(
                    if (isHeld) "HELD" else "Not Held",
                    style = MaterialTheme.typography.titleSmall,
                    fontWeight = FontWeight.Bold,
                    color = if (isHeld) MaterialTheme.colorScheme.onErrorContainer
                    else MaterialTheme.colorScheme.onSecondaryContainer
                )
                if (isHeld) {
                    hold.holdType?.let { Text("Type: $it", style = MaterialTheme.typography.bodySmall) }
                    hold.reason?.let { Text("Reason: $it", style = MaterialTheme.typography.bodySmall) }
                    hold.heldBy?.let { Text("By: $it", style = MaterialTheme.typography.bodySmall) }
                }
            }
        }
    }
}

@Composable
private fun LabeledRow(label: String, value: String) {
    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
        Text(label, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
        Text(value, style = MaterialTheme.typography.bodyMedium)
    }
}
