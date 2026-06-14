package com.iotviet.aeromes.ui.quality

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Card
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.iotviet.aeromes.domain.model.InspectionOrder

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun QualityDetailScreen(
    orderId: Int,
    onBack: () -> Unit,
    vm: QualityViewModel = hiltViewModel()
) {
    val state by vm.detailState.collectAsStateWithLifecycle()

    LaunchedEffect(orderId) { vm.loadDetail(orderId) }
    LaunchedEffect(state.actionSuccess) { if (state.actionSuccess) onBack() }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Inspection #$orderId") },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = null)
                    }
                }
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier.fillMaxSize().padding(padding).padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            when {
                state.isLoading -> CircularProgressIndicator(modifier = Modifier.align(Alignment.CenterHorizontally))
                state.error != null -> Text(state.error!!, color = MaterialTheme.colorScheme.error)
                state.order != null -> InspectionContent(
                    order = state.order!!,
                    isActing = state.isActing,
                    onStart = { vm.startInspection(orderId) },
                    onPass = { vm.passInspection(orderId) },
                    onFail = { vm.failInspection(orderId) }
                )
            }
        }
    }
}

@Composable
private fun InspectionContent(
    order: InspectionOrder,
    isActing: Boolean,
    onStart: () -> Unit,
    onPass: () -> Unit,
    onFail: () -> Unit
) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
            Text(order.orderCode, style = MaterialTheme.typography.titleLarge)
            Text("Product: ${order.productName}", style = MaterialTheme.typography.bodyMedium)
            Text("Code: ${order.productCode}", style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant)
            order.assignedTo?.let { Text("Assigned: $it", style = MaterialTheme.typography.bodySmall) }
        }
    }

    if (isActing) {
        CircularProgressIndicator()
    } else {
        val status = order.status.lowercase()
        if (status == "pending") {
            Button(onClick = onStart, modifier = Modifier.fillMaxWidth()) { Text("Start Inspection") }
        }
        if (status == "in_progress" || status == "started") {
            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                Button(
                    onClick = onPass,
                    modifier = Modifier.weight(1f),
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.tertiary)
                ) { Text("Pass") }
                OutlinedButton(
                    onClick = onFail,
                    modifier = Modifier.weight(1f),
                    colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error)
                ) { Text("Fail") }
            }
        }
    }
}
