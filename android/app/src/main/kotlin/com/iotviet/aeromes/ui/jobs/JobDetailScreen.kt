package com.iotviet.aeromes.ui.jobs

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
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.iotviet.aeromes.R
import com.iotviet.aeromes.domain.model.Job

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun JobDetailScreen(
    jobId: Long,
    onLogProduction: () -> Unit,
    onBack: () -> Unit,
    vm: JobViewModel = hiltViewModel()
) {
    val state by vm.detailState.collectAsStateWithLifecycle()

    LaunchedEffect(jobId) { vm.loadDetail(jobId) }
    LaunchedEffect(state.finishSuccess) { if (state.finishSuccess) onBack() }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Job #$jobId") },
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
                state.job != null -> JobDetailContent(
                    job = state.job!!,
                    isFinishing = state.isFinishing,
                    onLogProduction = onLogProduction,
                    onFinish = { vm.finishJob(jobId) }
                )
            }
        }
    }
}

@Composable
private fun JobDetailContent(
    job: Job,
    isFinishing: Boolean,
    onLogProduction: () -> Unit,
    onFinish: () -> Unit
) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
            DetailRow("Status", job.status)
            DetailRow(stringResource(R.string.label_machine), job.machineCode)
            DetailRow(stringResource(R.string.label_shift), job.shiftCode)
            DetailRow("Started", job.startTime)
            job.endTime?.let { DetailRow("Ended", it) }
        }
    }

    Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
        Button(onClick = onLogProduction, modifier = Modifier.weight(1f)) {
            Text("Log Production")
        }
        OutlinedButton(
            onClick = onFinish,
            enabled = !isFinishing && job.status.lowercase() != "completed",
            modifier = Modifier.weight(1f),
            colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error)
        ) {
            if (isFinishing) CircularProgressIndicator(modifier = Modifier.padding(4.dp))
            else Text("Finish Job")
        }
    }
}

@Composable
private fun DetailRow(label: String, value: String) {
    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
        Text(label, style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.onSurfaceVariant)
        Text(value, style = MaterialTheme.typography.bodyMedium)
    }
}
