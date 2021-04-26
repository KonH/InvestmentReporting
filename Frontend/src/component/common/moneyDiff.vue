<template>
	<span :style="style"
		>{{ sign }}<money :value="priceValueRound" :currency-code="currencyCode" /> <template v-if="hasPercent">({{ percent }}%)</template></span
	>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import Money from '@/component/common/money.vue';

@Options({
	name: 'MoneyDiff',
	components: {
		Money,
	},
})
export default class MoneyDiff extends Vue {
	@Prop()
	old!: number;

	@Prop()
	new!: number;

	@Prop()
	currencyCode!: string;

	get priceValue() {
		return this.new - this.old;
	}

	get priceValueRound() {
		return this.priceValue.toFixed(2);
	}

	get ratio() {
		const ratio = this.new / this.old - 1;
		return ratio * 100;
	}

	get percent() {
		return Math.abs(this.ratio).toFixed(2);
	}

	get hasPercent() {
		return Number.isFinite(this.ratio) && this.ratio != 0;
	}

	get sign() {
		return this.priceValue > 0 ? '+' : '';
	}

	get color() {
		if (this.priceValue == 0) {
			return 'black';
		}
		return this.priceValue >= 0 ? 'green' : 'red';
	}

	get style() {
		return `color: ${this.color}`;
	}
}
</script>
