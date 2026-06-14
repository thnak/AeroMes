package com.iotviet.aeromes.ui.material

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.iotviet.aeromes.domain.model.InventoryItem
import com.iotviet.aeromes.domain.model.WorkOrder
import com.iotviet.aeromes.domain.repository.InventoryRepository
import com.iotviet.aeromes.domain.repository.MaterialRepository
import com.iotviet.aeromes.domain.repository.WorkRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

enum class MaterialIssueStep { SELECT_WO, SCAN_LOT, CONFIRM }

data class MaterialIssueUiState(
    val step: MaterialIssueStep = MaterialIssueStep.SELECT_WO,
    // Step 1
    val workOrders: List<WorkOrder> = emptyList(),
    val selectedWo: WorkOrder? = null,
    val isLoadingWo: Boolean = false,
    // Step 2
    val lotNumber: String = "",
    val lotItem: InventoryItem? = null,
    val quantity: String = "",
    val notes: String = "",
    val isLoadingLot: Boolean = false,
    // Common
    val isSubmitting: Boolean = false,
    val error: String? = null,
    val successMessage: String? = null
)

@HiltViewModel
class MaterialIssueViewModel @Inject constructor(
    private val workRepository: WorkRepository,
    private val inventoryRepository: InventoryRepository,
    private val materialRepository: MaterialRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(MaterialIssueUiState())
    val uiState = _uiState.asStateFlow()

    init { loadWorkOrders() }

    fun onLotNumberChange(value: String) = _uiState.update { it.copy(lotNumber = value) }
    fun onQuantityChange(value: String) = _uiState.update { it.copy(quantity = value) }
    fun onNotesChange(value: String) = _uiState.update { it.copy(notes = value) }
    fun clearSuccess() = _uiState.update { it.copy(successMessage = null) }

    private fun loadWorkOrders() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoadingWo = true, error = null) }
            workRepository.getWorkOrders(status = "Released")
                .onSuccess { wos -> _uiState.update { it.copy(workOrders = wos, isLoadingWo = false) } }
                .onFailure { e -> _uiState.update { it.copy(isLoadingWo = false, error = e.message) } }
        }
    }

    fun selectWorkOrder(wo: WorkOrder) {
        _uiState.update { it.copy(selectedWo = wo, step = MaterialIssueStep.SCAN_LOT, error = null) }
    }

    fun lookupLot() {
        val lotNumber = _uiState.value.lotNumber.trim()
        if (lotNumber.isBlank()) return
        viewModelScope.launch {
            _uiState.update { it.copy(isLoadingLot = true, error = null, lotItem = null) }
            inventoryRepository.getInventory(lotNumber = lotNumber)
                .onSuccess { items ->
                    val item = items.firstOrNull()
                    if (item == null) {
                        _uiState.update { it.copy(isLoadingLot = false, error = "Lot not found.") }
                    } else {
                        _uiState.update { it.copy(isLoadingLot = false, lotItem = item, quantity = item.quantity.toString()) }
                    }
                }
                .onFailure { e -> _uiState.update { it.copy(isLoadingLot = false, error = e.message) } }
        }
    }

    fun proceedToConfirm() {
        val state = _uiState.value
        if (state.lotItem == null) { _uiState.update { it.copy(error = "Look up a lot first.") }; return }
        val qty = state.quantity.toDoubleOrNull()
        if (qty == null || qty <= 0) { _uiState.update { it.copy(error = "Enter a valid quantity.") }; return }
        _uiState.update { it.copy(step = MaterialIssueStep.CONFIRM, error = null) }
    }

    fun goBack() {
        _uiState.update {
            when (it.step) {
                MaterialIssueStep.SCAN_LOT -> it.copy(step = MaterialIssueStep.SELECT_WO, error = null)
                MaterialIssueStep.CONFIRM -> it.copy(step = MaterialIssueStep.SCAN_LOT, error = null)
                else -> it
            }
        }
    }

    fun submit() {
        val state = _uiState.value
        val wo = state.selectedWo ?: return
        val lotItem = state.lotItem ?: return
        val qty = state.quantity.toDoubleOrNull() ?: return
        viewModelScope.launch {
            _uiState.update { it.copy(isSubmitting = true, error = null) }
            materialRepository.issueMaterial(
                woId = wo.id,
                productCode = lotItem.productCode,
                lotNumber = lotItem.lotNumber ?: state.lotNumber.trim(),
                quantity = qty,
                notes = state.notes.trim().takeIf { n -> n.isNotBlank() }
            )
                .onSuccess {
                    val currentWos = _uiState.value.workOrders
                    _uiState.update {
                        MaterialIssueUiState(
                            workOrders = currentWos,
                            successMessage = "Material issued successfully."
                        )
                    }
                }
                .onFailure { e -> _uiState.update { it.copy(isSubmitting = false, error = e.message) } }
        }
    }
}
