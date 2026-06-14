package com.iotviet.aeromes.ui.maintenance

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.iotviet.aeromes.domain.repository.MaintenanceRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class MachineFaultUiState(
    val machineCode: String = "",
    val description: String = "",
    val priority: String = "Normal",
    val requestedBy: String = "",
    val isSubmitting: Boolean = false,
    val error: String? = null,
    val successMessage: String? = null
)

val faultPriorities = listOf("Low", "Normal", "High", "Critical")

@HiltViewModel
class MachineFaultViewModel @Inject constructor(
    private val maintenanceRepository: MaintenanceRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(MachineFaultUiState())
    val uiState = _uiState.asStateFlow()

    fun onMachineCodeChange(value: String) = _uiState.update { it.copy(machineCode = value) }
    fun onDescriptionChange(value: String) = _uiState.update { it.copy(description = value) }
    fun onPriorityChange(value: String) = _uiState.update { it.copy(priority = value) }
    fun onRequestedByChange(value: String) = _uiState.update { it.copy(requestedBy = value) }
    fun clearSuccess() = _uiState.update { it.copy(successMessage = null) }

    fun submit() {
        val state = _uiState.value
        val machineCode = state.machineCode.trim()
        val description = state.description.trim()
        val requestedBy = state.requestedBy.trim()
        if (machineCode.isBlank() || description.isBlank() || requestedBy.isBlank()) {
            _uiState.update { it.copy(error = "Machine code, description, and requestedBy are required.") }
            return
        }
        viewModelScope.launch {
            _uiState.update { it.copy(isSubmitting = true, error = null) }
            maintenanceRepository.createMaintenanceOrder(machineCode, description, state.priority, requestedBy)
                .onSuccess {
                    _uiState.update {
                        it.copy(
                            isSubmitting = false,
                            successMessage = "Maintenance order created successfully.",
                            machineCode = "",
                            description = "",
                            requestedBy = "",
                            priority = "Normal"
                        )
                    }
                }
                .onFailure { e -> _uiState.update { it.copy(isSubmitting = false, error = e.message) } }
        }
    }
}
