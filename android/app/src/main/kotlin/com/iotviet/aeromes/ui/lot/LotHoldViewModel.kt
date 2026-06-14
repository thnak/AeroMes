package com.iotviet.aeromes.ui.lot

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.iotviet.aeromes.domain.model.HoldStatus
import com.iotviet.aeromes.domain.repository.LotRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class LotHoldUiState(
    val lotNumber: String = "",
    val holdStatus: HoldStatus? = null,
    val reason: String = "",
    val holdType: String = "Quality",
    val isLoadingStatus: Boolean = false,
    val isSubmitting: Boolean = false,
    val error: String? = null,
    val successMessage: String? = null
)

val holdTypes = listOf("Quality", "Safety", "Damaged", "Expired", "Other")

@HiltViewModel
class LotHoldViewModel @Inject constructor(
    private val lotRepository: LotRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(LotHoldUiState())
    val uiState = _uiState.asStateFlow()

    fun onLotNumberChange(value: String) = _uiState.update { it.copy(lotNumber = value) }
    fun onReasonChange(value: String) = _uiState.update { it.copy(reason = value) }
    fun onHoldTypeChange(value: String) = _uiState.update { it.copy(holdType = value) }
    fun clearSuccess() = _uiState.update { it.copy(successMessage = null) }

    fun prefill(lotNumber: String) {
        _uiState.update { it.copy(lotNumber = lotNumber) }
        loadStatus(lotNumber)
    }

    fun loadStatus(lotNumber: String = _uiState.value.lotNumber.trim()) {
        if (lotNumber.isBlank()) return
        viewModelScope.launch {
            _uiState.update { it.copy(isLoadingStatus = true, error = null, holdStatus = null) }
            lotRepository.getLotHoldStatus(lotNumber)
                .onSuccess { status -> _uiState.update { it.copy(isLoadingStatus = false, holdStatus = status) } }
                .onFailure { e -> _uiState.update { it.copy(isLoadingStatus = false, error = e.message) } }
        }
    }

    fun placeHold() {
        val state = _uiState.value
        val lotNumber = state.lotNumber.trim()
        val reason = state.reason.trim()
        if (lotNumber.isBlank() || reason.isBlank()) {
            _uiState.update { it.copy(error = "Lot number and reason are required.") }
            return
        }
        viewModelScope.launch {
            _uiState.update { it.copy(isSubmitting = true, error = null) }
            lotRepository.placeHold(lotNumber, reason, state.holdType)
                .onSuccess {
                    _uiState.update {
                        it.copy(isSubmitting = false, successMessage = "Hold placed successfully.", reason = "", holdStatus = null)
                    }
                    loadStatus(lotNumber)
                }
                .onFailure { e -> _uiState.update { it.copy(isSubmitting = false, error = e.message) } }
        }
    }

    fun releaseHold() {
        val state = _uiState.value
        val holdId = state.holdStatus?.holdId ?: return
        val reason = state.reason.trim()
        if (reason.isBlank()) {
            _uiState.update { it.copy(error = "Release reason is required.") }
            return
        }
        viewModelScope.launch {
            _uiState.update { it.copy(isSubmitting = true, error = null) }
            lotRepository.releaseHold(holdId, reason)
                .onSuccess {
                    _uiState.update {
                        it.copy(isSubmitting = false, successMessage = "Hold released successfully.", reason = "", holdStatus = null)
                    }
                    loadStatus(state.lotNumber.trim())
                }
                .onFailure { e -> _uiState.update { it.copy(isSubmitting = false, error = e.message) } }
        }
    }
}
