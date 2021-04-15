<template>
	<div>
		<h2>Currencies</h2>
		<div class="row pl-3">
			<div v-for="currency in currencies" :key="currency.id">
				<currency-card :currency="currency" class="mr-2" />
			</div>
			<add-currency-card />
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import CurrencyCard from '@/component/currencyCard.vue';
import AddCurrencyCard from '@/component/addCurrencyCard.vue';
import { StateDto } from '@/api/state';
import { State } from 'vuex-class';

@Options({
	name: 'CurrenciesView',
	components: {
		CurrencyCard,
		AddCurrencyCard,
	},
})
export default class CurrenciesView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	get currencies() {
		return this.activeState.currencies;
	}
}
</script>
