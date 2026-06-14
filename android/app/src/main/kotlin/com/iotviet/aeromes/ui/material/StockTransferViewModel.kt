package com.iotviet.aeromes.ui.material

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.iotviet.aeromes.domain.model.LotTrace
import com.iotviet.aeromes.domain.model.StorageLocation
import com.iotviet.aeromes.domain.repository.LotRepository
import com.iotviet.aeromes.domain.repository.MaterialRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

enum class StockTransferStep { SCAN_LOT, PICK_LOCATION, CONFIRM }

data class StockTransferUiState(
    val step: StockTransferStep = StockTransferStep.SCAN_LOT,
    // Step 1
    val lotNumber: String = "",
    val lotTrace: LotTrace? = null,
    val isLoadingLot: Boolean = false,
    // Step 2
    val locations: List<StorageLocation> = emptyList(),
    val selectedLocation: StorageLocation? = null,
    val isLoadingLocations: Boolean = false,
    // Step 3
    val quantity: String = "",
    // Common
    val isSubmitting: Boolean = false,
    val error: String? = null,
    val successMessage: String? = null
)

@HiltViewModel
class StockTransferViewModel @Inject constructor(
    private val lotRepository: LotRepository,
    private val materialRepository: MaterialRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(StockTransferUiState())
    val uiState = _uiState.asStateFlow()

    fun onLotNumberChange(value: String) = _uiState.update { it.copy(lotNumber = value) }
    fun onQuantityChange(value: String) = _uiState.update { it.copy(quantity = value) }
    fun clearSuccess() = _uiState.update { it.copy(successMessage = null) }

    fun lookupLot() {
        val lotNumber = _uiState.value.lotNumber.trim()
        if (lotNumber.isBlank()) return
        viewModelScope.launch {
            _uiState.update { it.copy(isLoadingLot = true, error = null, lotTrace = null) }
            lotRepository.getLotTrace(lotNumber)
                .onSuccess { trace ->
                    _uiState.update {
                        it.copy(
                            isLoadingLot = false,
                            lotTrace = trace,
                            quantity = trace.quantity.toString()
                        )
                    }
                }
                .onFailure { e -> _uiState.update { it.copy(isLoadingLot = false, error = e.message) } }
        }
    }

    fun proceedToPickLocation() {
        if (_uiState.value.lotTrace == null) {
            _uiState.update { it.copy(error = "Look up a lot first.") }
            return
        }
        val qty = _uiState.value.quantity.toDoubleOrNull()
        if (qty == null || qty <= 0) {
            _uiState.update { it.copy(error = "Enter a valid quantity.") }
            return
        }
        viewModelScope.launch {
            _uiState.update { it.copy(step = StockTransferStep.PICK_LOCATION, error = null, isLoadingLocations = true) }
            materialRepository.getStorageLocations()
                .onSuccess { locs -> _uiState.update { it.copy(locations = locs, isLoadingLocations = false) } }
                .onFailure { e -> _uiState.update { it.copy(isLoadingLocations = false, error = e.message) } }
        }
    }

    fun selectLocation(location: StorageLocation) {
        _uiState.update { it.copy(selectedLocation = location, step = StockTransferStep.CONFIRM, error = null) }
    }

    fun goBack() {
        _uiState.update {
            when (it.step) {
                StockTransferStep.PICK_LOCATION -> it.copy(step = StockTransferStep.SCAN_LOT, error = null)
                StockTransferStep.CONFIRM -> it.copy(step = StockTransferStep.PICK_LOCATION, error = null)
                else -> it
            }
        }
    }

    fun submit() {
        val state = _uiState.value
        val trace = state.lotTrace ?: return
        val destLocation = state.selectedLocation ?: return
        val qty = state.quantity.toDoubleOrNull() ?: return
        val sourceLocationId = trace.locationCode ?: run {
            _uiState.update { it.copy(error = "Source location not found for this lot.") }
            return
        }
        viewModelScope.launch {
            _uiState.update { it.copy(isSubmitting = true, error = null) }
            materialRepository.transferStock(
                lotNumber = trace.lotNumber,
                sourceLocationId = sourceLocationId,
                destinationLocationId = destLocation.id,
                quantity = qty
            )
                .onSuccess {
                    _uiState.update {
                        StockTransferUiState(successMessage = "Stock transferred successfully.")
                    }
                }
                .onFailure { e -> _uiState.update { it.copy(isSubmitting = false, error = e.message) } }
        }
    }
}
