package com.iotviet.aeromes.ui.lot

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.iotviet.aeromes.domain.model.HoldStatus
import com.iotviet.aeromes.domain.model.LotTrace
import com.iotviet.aeromes.domain.repository.LotRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.async
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class LotLookupUiState(
    val lotNumber: String = "",
    val lotTrace: LotTrace? = null,
    val holdStatus: HoldStatus? = null,
    val isLoading: Boolean = false,
    val error: String? = null
)

@HiltViewModel
class LotLookupViewModel @Inject constructor(
    private val lotRepository: LotRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(LotLookupUiState())
    val uiState = _uiState.asStateFlow()

    fun onLotNumberChange(value: String) = _uiState.update { it.copy(lotNumber = value) }

    fun lookup(lotNumber: String = _uiState.value.lotNumber.trim()) {
        if (lotNumber.isBlank()) return
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, error = null, lotTrace = null, holdStatus = null) }
            val traceDeferred = async { lotRepository.getLotTrace(lotNumber) }
            val holdDeferred = async { lotRepository.getLotHoldStatus(lotNumber) }
            val traceResult = traceDeferred.await()
            val holdResult = holdDeferred.await()
            if (traceResult.isFailure) {
                _uiState.update { it.copy(isLoading = false, error = traceResult.exceptionOrNull()?.message) }
            } else {
                _uiState.update {
                    it.copy(
                        isLoading = false,
                        lotTrace = traceResult.getOrNull(),
                        holdStatus = holdResult.getOrNull()
                    )
                }
            }
        }
    }

    fun prefill(lotNumber: String) {
        _uiState.update { it.copy(lotNumber = lotNumber) }
        lookup(lotNumber)
    }
}
